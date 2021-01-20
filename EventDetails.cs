using System;
using Microsoft.Exchange.WebServices.Data;

namespace ExchangeApp
{
    public sealed class EventDetails
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ZoomLink { get; set; }
        public long ZoomId { get; set; }

        /// <summary>
        /// Formats the Zoom Id of a meeting in the same way that Zoom displays them.
        /// </summary>
        public string FormattedZoomId
        {
            get
            {
                if (ZoomId == default)
                    return "none";

                return ZoomId.ToString()
                    .Insert(3, " ")
                    .Insert(ZoomId.ToString().Length > 11 ? 8 : 7, " ");
            }
        }

        public MeetingResponseType Response { get; set; }

        /// <summary>
        /// To have a somewhat useful order in which meetings are picked to double-click, or first in the list, if both
        /// are scheduled for the same time, there's an explicit order defined here based on the response sent by the
        /// user.
        ///
        /// For example, appointments that we have accepted or created are more important than appointments we declined.
        /// </summary>
        public int MeetingResponseOrder
        {
            get
            {
                switch (Response)
                {
                    case MeetingResponseType.Accept:
                    case MeetingResponseType.Organizer:
                        return 0;

                    case MeetingResponseType.Tentative:
                        return 1;

                    case MeetingResponseType.Unknown:
                    case MeetingResponseType.NoResponseReceived:
                        return 2;

                    case MeetingResponseType.Decline:
                        return 3;
                }

                return Int32.MaxValue;
            }
        }

        public override string ToString() => ToString(true, true);

        public string ToString(bool includeMeetingId, bool includeTime)
        {
            string original = includeTime ? $"{Start:HH:mm} - {Subject}" : Subject;
            string formatted = original;
            if (ZoomId != default && includeMeetingId)
                formatted += $" ({FormattedZoomId})";

            if (formatted.Length < 64)
                return formatted;

            return original.Length < 63 ? original : original.Substring(0, 63);
        }
    }
}
