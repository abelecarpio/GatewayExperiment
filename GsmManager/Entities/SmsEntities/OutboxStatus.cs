using System;

namespace GsmManager.Entities.SmsEntities
{
    public class OutboxStatus
    {
        public int MessageId { get; set; }
        public DateTime SentOn { get; set; }
        public bool IsSuccess { get; set; }
    }
}