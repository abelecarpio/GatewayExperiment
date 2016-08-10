using System;

namespace GsmManager.Entities.SmsEntities
{
    public class Inbox
    {
        public int MessageId { get; set; }
        public string MobileNumber { get; set; }
        public string SmsMessage { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime RetrievedOn { get; set; }
        public bool IsRead { get; set; }
    }
}