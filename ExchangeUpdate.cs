using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace ExchangeApp
{
    /// <summary>
    /// Query EWS regularly for new updates to calendar entries.
    /// </summary>
    public sealed class ExchangeUpdate
    {
        /// <summary>
        /// How often <see cref="Update"/> is called to update the list of displayed calendar entries.
        /// </summary>
        private readonly TimeSpan _trayUpdateInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// How often the EWS server is queried for <see cref="FetchCalendarEntries"/>,
        /// even if <see cref="Update"/> is (obviously) called more often.
        /// </summary>
        private readonly TimeSpan _exchangeUpdateInterval = TimeSpan.FromMinutes(5);

        private readonly TimeSpan _latestEndForStickyEvents = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30));

        private readonly ExchangeService _exchange;
        private readonly INotificationCallback _notificationCallback;

        public ExchangeUpdate(INotificationCallback notificationCallback)
        {
            _notificationCallback = notificationCallback;
            _exchange = new ExchangeService(ExchangeVersion.Exchange2013_SP1)
            {
                TraceEnabled = false,
                PreferredCulture = CultureInfo.CurrentCulture
            };

            Timer timer = new Timer
            {
                Interval = _trayUpdateInterval.TotalMilliseconds,
                Enabled = true,
                AutoReset = true,
            };
            timer.Elapsed += (sender, e) => Update();
            timer.Start();
        }

        private IReadOnlyList<EventDetails> UpcomingEvents { get; set; } = new List<EventDetails>();
        private IReadOnlyList<EventDetails> StickyEvents { get; set; } = new List<EventDetails>();


        /// <summary>
        /// Debug-Only: Shows when the last call to <see cref="Update"/> was made.
        /// </summary>
        public DateTime LastUpdate { get; private set; }

        /// <summary>
        /// Debug-Only: Shows when the last call to <see cref="FetchCalendarEntries"/> was made.
        /// </summary>
        public DateTime LastFetch { get; private set; }

        public void Update(bool force = false)
        {
            _notificationCallback.OnRefresh();

            Task.Run(() =>
            {
                try
                {
                    if (force || DateTime.Now > LastFetch.Add(_exchangeUpdateInterval))
                    {
                        FetchCalendarEntries();
                    }

                    // We realistically care more about events about to start (or just started) vs. already ending events,
                    // and this is done in a way where we don't need to query Exchange every time we update the list.
                    UpcomingEvents = UpcomingEvents
                        .Where(x => x.End > DateTime.Now.AddMinutes(-5))
                        .ToList()
                        .AsReadOnly();

                    LastUpdate = DateTime.Now;
                    _notificationCallback.OnUpdate(UpcomingEvents, StickyEvents);
                }
                catch (Exception e)
                {
                    _notificationCallback.OnError(e);
                }
            });
        }

        /// <summary>
        /// Loads all calendar entries that should be displayed.
        /// </summary>
        private void FetchCalendarEntries()
        {
            FetchUpcomingEvents();
            FetchStickyEntries();
            LastFetch = DateTime.Now;
        }

        /// <summary>
        /// Loads the upcoming calendar entries: Anything from now until the end of today, at most <paramref name="maxAppointments"/> items.
        /// </summary>
        /// <param name="maxAppointments">Max number of items to load</param>
        /// <seealso cref="FetchStickyEntries"/>
        /// <seealso cref="IsSticky"/>
        private void FetchUpcomingEvents(int maxAppointments = 10)
        {
            CalendarFolder calendar = CalendarFolder.Bind(_exchange, WellKnownFolderName.Calendar);
            CalendarView view = new CalendarView(DateTime.Now.AddMinutes(-5), DateTime.Today.AddDays(1).AddSeconds(-1), maxAppointments)
            {
                PropertySet = new PropertySet(ItemSchema.Subject, AppointmentSchema.Start, AppointmentSchema.End, AppointmentSchema.Location, AppointmentSchema.MyResponseType, AppointmentSchema.IsAllDayEvent)
            };
            FindItemsResults<Appointment> appointments = calendar.FindAppointments(view);

            UpcomingEvents = appointments
                .Where(x => !IsSticky(x))
                .Select(ConvertToEvent)
                .OrderBy(x => x.Start)
                .ThenBy(x => x.MeetingResponseOrder)
                .ThenBy(x => x.End)
                .ThenBy(x => x.Subject)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Loads all of today's sticky entries.
        /// </summary>
        /// <seealso cref="FetchUpcomingEvents"/>
        /// <seealso cref="IsSticky"/>
        private void FetchStickyEntries()
        {
            CalendarFolder calendar = CalendarFolder.Bind(_exchange, WellKnownFolderName.Calendar);
            CalendarView view = new CalendarView(DateTime.Today, DateTime.Today.Add(_latestEndForStickyEvents))
            {
                PropertySet = new PropertySet(ItemSchema.Subject, AppointmentSchema.Start, AppointmentSchema.End, AppointmentSchema.Location, AppointmentSchema.MyResponseType, AppointmentSchema.IsAllDayEvent)
            };
            FindItemsResults<Appointment> appointments = calendar.FindAppointments(view);

            StickyEvents = appointments
                .Where(IsSticky)
                .Select(ConvertToEvent)
                .Where(x => x.ZoomId != default)
                .OrderBy(x => x.Subject)
                .ThenBy(x => x.MeetingResponseOrder)
                .ThenBy(x => x.Start)
                .ThenBy(x => x.End)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Determines if an <paramref name="appointment"/> will be displayed a sticky - with a special symbol,
        /// always on top. This is primarily intended for persistent Zoom "groups" where you can join any time of the day.
        /// </summary>
        private bool IsSticky(Appointment appointment) => !appointment.IsAllDayEvent && appointment.Start.TimeOfDay <= _latestEndForStickyEvents;

        /// <summary>
        /// Wraps an exchange <paramref name="appointment"/> into a more compact <see cref="EventDetails"/>.
        /// </summary>
        private EventDetails ConvertToEvent(Appointment appointment)
        {
            EventDetails details = new EventDetails
            {
                Id = appointment.Id.UniqueId,
                Subject = appointment.Subject,
                Start = appointment.Start,
                End = appointment.End,
                Response = appointment.MyResponseType,
            };

            if (TryParseZoomLink(appointment.Location, out string zoomLink, out long zoomId))
            {
                details.ZoomLink = zoomLink;
                details.ZoomId = zoomId;
            }
            else
            {
                appointment.Load(new PropertySet(ItemSchema.TextBody));
                if (TryParseZoomLink(appointment.TextBody, out zoomLink, out zoomId))
                {
                    details.ZoomLink = zoomLink;
                    details.ZoomId = zoomId;
                }
            }

            return details;
        }

        /// <summary>
        /// Tries to parse a zoom web link like
        ///
        /// <code>https://zoom.us/j/[id]?pwd=[pass]</code>
        /// to
        /// <code>zoommtg://zoom.us/join?action=join&amp;confno=[id]&amp;pwd=[pass]</code>
        /// </summary>
        /// <param name="rawText">Either the appointment's location or text</param>
        /// <param name="zoomLink">Zoom link, if any was found</param>
        /// <param name="zoomId">Zoom meeting id, if any was found</param>
        /// <returns>True if a Zoom link/meeting id were found, false otherwise</returns>
        private bool TryParseZoomLink(string rawText, out string zoomLink, out long zoomId)
        {
            try
            {
                var match = Regex.Match(rawText, @"https://zoom.us/j/(?<id>\d+)\?pwd=(?<pass>[a-zA-Z0-9]+)");
                if (match.Success)
                {
                    zoomId = long.Parse(match.Groups["id"].Value);
                    zoomLink = $"zoommtg://zoom.us/join?action=join&confno={zoomId}&pwd={match.Groups["pass"].Value}";
                    return true;
                }

                match = Regex.Match(rawText, @"https://zoom.us/j/(?<id>\d+)");
                if (match.Success)
                {
                    zoomId = long.Parse(match.Groups["id"].Value);
                    zoomLink = $"zoommtg://zoom.us/join?action=join&confno={zoomId}";
                    return true;
                }
            }
            catch (Exception)
            {
                // we couldn't extract the Zoom link, but so what?
            }

            zoomId = 0;
            zoomLink = null;
            return false;
        }

        /// <summary>
        /// Whenever the user updates the setting, we want to reflect the change for the EWS connection.
        /// </summary>
        /// <param name="settings">newly valid settings</param>
        public void ApplySettings(Settings settings)
        {
            _exchange.Url = null;
            _exchange.Credentials = null;

            try
            {
                _exchange.Url = new Uri(settings.ExchangeUrl);
            }
            catch (Exception e) when (e is ArgumentNullException || e is UriFormatException)
            {
                _exchange.Url = null;
            }

            if (!string.IsNullOrEmpty(settings.User))
            {
                _exchange.Credentials = new WebCredentials(settings.User, settings.Password);
                _exchange.UseDefaultCredentials = false;
            }
            else
            {
                _exchange.Credentials = null;
                _exchange.UseDefaultCredentials = true;
            }
        }
    }
}
