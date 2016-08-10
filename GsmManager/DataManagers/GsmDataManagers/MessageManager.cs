using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GsmManager.Entities.GsmEntities;

namespace GsmManager.DataManagers.GsmDataManagers
{
    public class MessageManager : IDisposable
    {
        private readonly List<MessageInfo> sample;

        public MessageManager()
        {
            sample = new List<MessageInfo>();
            for (var i = 0; i < 100; i++)
            {
                sample.Add(new MessageInfo() { MessageId = i, Sender = "+6391212345678", Message = i + new string('x', 120), ReceiveDate = DateTime.Now.AddSeconds(i) });
            }
            sample = sample.OrderByDescending(info => info.MessageId).ToList();
        }

        public IList<MessageInfo> GetMessage(int lastId, string searchValue)
        {
            var query = sample.Where(info => (lastId == -1 || info.MessageId < lastId));
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(info => info.Sender.Contains(searchValue) || info.Message.Contains(searchValue));
            }

            return query.Take(10).ToList();
        }

        public void SendMessage(string mobileNumber, string message)
        {
            
        }

        public void Dispose()
        {
            
        }

        public void SetReadMessageStatus(int messageId, char status)
        {
            var messageInfo = sample.First(info => info.MessageId == messageId);
            messageInfo.Status = status;
        }

        public IList<MessageInfo> GetMessage(DateTime lastMessageReceiveDateTime)
        {
            return sample.Where(info => info.ReceiveDate > lastMessageReceiveDateTime).ToList();
        }
    }
}
