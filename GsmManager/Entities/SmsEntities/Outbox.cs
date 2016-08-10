using System;

namespace GsmManager.Entities.SmsEntities
{
    public class Outbox
    {
        public int MessageId { get; set; }
        public string MobileNumber { get; set; }
        public string SmsMessage { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsUrgent { get; set; }
    }
}