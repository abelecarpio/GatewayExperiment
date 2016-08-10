using System;
using System.Text.RegularExpressions;

namespace GsmUtilities.Models
{
    public class SmsMessage
    {
        private const int MESSAGE_DISPLAY_LENGTH = 20;

        public SmsMessage()
        {
            var guid = Guid.NewGuid();
            MessageId = string.Format("{0}-{1}", guid.ToString(), DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fffffff"));
        }

        public string MobileNumber { get; set; }
        public string TextMessage { get; set; }
        public MessagePriority Priority { get; set; }
        public int RetryCount { get; set; }

        public string MessageDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(TextMessage)) return string.Empty;
                return TextMessage.Length <= MESSAGE_DISPLAY_LENGTH
                    ? TextMessage
                    : string.Format("{0}...", TextMessage.Substring(0, MESSAGE_DISPLAY_LENGTH - 3));
            }
        }

        public readonly string MessageId;


        internal DateTime LocalReceivedOn { get; set; }
        public DateTime ReceivedOn { get { return LocalReceivedOn; } }

        internal int ReferenceId { get; set; }
    }
}