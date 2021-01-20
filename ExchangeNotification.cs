using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ExchangeApp.Localization;
using ExchangeApp.Properties;

namespace ExchangeApp
{
    /// <summary>
    /// Callbacks for any updates happening in <see cref="ExchangeUpdate"/>.
    /// </summary>
    public interface INotificationCallback
    {
        /// <summary>
        /// The refresh was triggered, and is waiting for completion.
        /// </summary>
        void OnRefresh();

        /// <summary>
        /// Loading data from the Exchange server failed, see <paramref name="exception"/> for details.
        /// </summary>
        /// <param name="exception">The exception we got.</param>
        void OnError(Exception exception);


        /// <summary>
        /// Querying EWS was successful, and we got new event lists.
        /// </summary>
        /// <param name="upcomingEvents">upcoming events in the near future, today</param>
        /// <param name="stickyEvents">persistent events to always display</param>
        void OnUpdate(IReadOnlyList<EventDetails> upcomingEvents, IReadOnlyList<EventDetails> stickyEvents);
    }

    /// <summary>
    /// Core notification implementation, responsible for managing the tray icon.
    /// </summary>
    public sealed class ExchangeNotification : IDisposable, INotificationCallback
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon
        {
            Text = Translations.Tray_Status_Connecting_to_Exchange,
            Visible = true,
        };
        private readonly Settings _settings;
        private readonly ExchangeUpdate _updater;

        public ExchangeNotification()
        {
            UpdateStatus(AppStatus.NoUpcomingEvents);

            _settings = Settings.Load();

            _updater = new ExchangeUpdate(this);
            _updater.ApplySettings(_settings);
            _updater.Update();

            _notifyIcon.DoubleClick += (sender, e) => OpenZoomLink(_notifyIcon.Tag);
            UpdateMenuItems(new List<EventDetails>(), new List<EventDetails>());
        }

        public bool IsVisible => _notifyIcon.Visible;

        /// <summary>
        /// Updates the appearance of the tray icon, depending on the current status.
        /// </summary>
        /// <param name="status">Current status</param>
        private void UpdateStatus(AppStatus status)
        {
            switch (status)
            {
                case AppStatus.NoUpcomingEvents:
                    _notifyIcon.Icon = Icons.bell_default;
                    break;

                case AppStatus.UpcomingEvent:
                    _notifyIcon.Icon = Icons.bell_yellow;
                    break;

                case AppStatus.Refresh:
                    _notifyIcon.Icon = Icons.bell_refresh;
                    break;

                case AppStatus.Error:
                    _notifyIcon.Icon = Icons.bell_error;
                    break;
            }
        }

        /// <inheritdoc cref="INotificationCallback.OnRefresh"/>
        public void OnRefresh() => UpdateStatus(AppStatus.Refresh);

        /// <inheritdoc cref="INotificationCallback.OnError"/>
        public void OnError(Exception exception)
        {
            _notifyIcon.Text = exception.Message.Length < 63 ? exception.Message : exception.Message.Substring(0, 63);
            _notifyIcon.Tag = exception;
            UpdateStatus(AppStatus.Error);
        }

        /// <inheritdoc cref="INotificationCallback.OnUpdate"/>
        public void OnUpdate(IReadOnlyList<EventDetails> upcomingEvents, IReadOnlyList<EventDetails> stickyEvents)
        {
            var nextEvent = upcomingEvents.FirstOrDefault(x => x.End > DateTime.Now && x.Start > DateTime.Now.AddMinutes(-10));
            _notifyIcon.Text = nextEvent?.ToString(_settings.ShowMeetingIds, true) ?? Translations.Tray_Status_No_upcoming_appointments;
            _notifyIcon.Tag = nextEvent?.ZoomLink;

            UpdateStatus(nextEvent != null && nextEvent.Start < DateTime.Now.AddMinutes(10) ? AppStatus.UpcomingEvent : AppStatus.NoUpcomingEvents);

            UpdateMenuItems(upcomingEvents, stickyEvents);
        }

        /// <summary>
        /// Updates the right click menu of our tray icon.
        /// </summary>
        private void UpdateMenuItems(IReadOnlyList<EventDetails> upcomingEvents, IReadOnlyList<EventDetails> stickyEvents)
        {
            ContextMenuStrip newMenu = new ContextMenuStrip();
            const string separator = "-";

            if (stickyEvents.Any())
            {
                foreach (EventDetails details in stickyEvents)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(details.ToString(_settings.ShowMeetingIds, false), Icons.pushpin_line);
                    item.Tag = details.ZoomLink;
                    item.Click += (sender, e) => OpenZoomLink(item.Tag);

                    newMenu.Items.Add(item);
                }

                newMenu.Items.Add(separator);
            }

            if (upcomingEvents.Any())
            {
                foreach (EventDetails details in upcomingEvents)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(details.ToString(_settings.ShowMeetingIds, true));
                    item.Image = details.ZoomId == default ? Icons.question_mark : null;
                    item.Tag = details.ZoomLink;
                    item.Click += (sender, e) => OpenZoomLink(item.Tag);

                    newMenu.Items.Add(item);
                }

                newMenu.Items.Add(separator);
            }

#if DEBUG
            if (_updater.LastFetch != default || _updater.LastUpdate != default)
            {
                string ShortenTime(DateTime t) => t.Date == DateTime.Today ? $"{t:T}" : $"{t:G}";

                newMenu.Items.Add($"Last EWS call: {ShortenTime(_updater.LastFetch)}", Icons.download_cloud_2_line);
                newMenu.Items.Add($"Last update: {ShortenTime(_updater.LastUpdate)}");
                newMenu.Items.Add(separator);
            }
#endif

            ToolStripMenuItem sync = new ToolStripMenuItem(Translations.Tray_Actions_Sync_now, Icons.refresh_line);
            sync.Click += (sender, e) => _updater.Update(true);
            newMenu.Items.Add(sync);

            ToolStripMenuItem config = new ToolStripMenuItem(Translations.Tray_Actions_Configuration);
            config.Click += OpenConfiguration;
            newMenu.Items.Add(config);

            ToolStripMenuItem exit = new ToolStripMenuItem(Translations.Tray_Actions_Exit, Icons.close_line);
            exit.Click += (sender, e) => _notifyIcon.Visible = false;
            newMenu.Items.Add(exit);

            _notifyIcon.ContextMenuStrip = newMenu;
        }

        /// <summary>
        /// Opens the zoom link associated with <paramref name="tag"/>, which happens both when double-clicking
        /// the tray icon as well as through the context menu.
        /// </summary>
        /// <param name="tag">Zoom link or exception to display</param>
        private void OpenZoomLink(object tag)
        {
            if (tag is string link)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true,
                });
            }
            else if (tag is Exception e)
            {
                MessageBox.Show(e.ToString(), e.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the configuration menu.
        /// </summary>
        private void OpenConfiguration(object sender, EventArgs e)
        {
            using (var form = new FormConfig { ExchangeUrl = _settings.ExchangeUrl, User = _settings.User, Password = _settings.Password, ShowMeetingIds = _settings.ShowMeetingIds })
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _settings.ExchangeUrl = form.ExchangeUrl;
                    _settings.User = form.User;
                    _settings.Password = form.Password;
                    _settings.ShowMeetingIds = form.ShowMeetingIds;
                    _settings.Save();

                    _updater.ApplySettings(_settings);
                    _updater.Update(true);
                }
            }
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}
