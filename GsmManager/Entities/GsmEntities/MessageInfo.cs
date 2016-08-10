using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsmManager.Entities.GsmEntities
{
    public class MessageInfo
    {
        public MessageInfo()
        {
            
        }

        public MessageInfo(string sender, string message, DateTime receiveDate): this (0, sender, message, receiveDate)
        {
        }

        public MessageInfo(int messageId,string sender, string message, DateTime receiveDate)
        {
            MessageId = messageId;
            Sender = sender;
            Message = message;
            ReceiveDate = receiveDate;
        }

        public int MessageId { get; set; }

        public string Sender { get; set; }

        public string Message { get; set; }

        public char Status { get; set; }

        public DateTime ReceiveDate { get; set; }
    }
}
