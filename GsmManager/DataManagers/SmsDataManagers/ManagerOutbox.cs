using GsmManager.Abstracts;
using GsmManager.Entities.SmsEntities;
using GsmManager.Queries;
using SqliteDatabaseManager;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace GsmManager.DataManagers.SmsDataManagers
{
    public class ManagerOutbox : SmsDatabaseWorker
    {
        public OperationStatus AddOutbox(string mobileNumber, string message, bool isurgent)
        {
            AddParameter(new SQLiteParameter(@"@IsUrgent", DbType.Boolean), isurgent);
            AddParameter(new SQLiteParameter(@"@MobileNumber", DbType.String), mobileNumber);
            AddParameter(new SQLiteParameter(@"@SmsMessage", DbType.String), message);
            return InsertUpdateItem(QueryOutbox.INSERT_OUTBOX, CommandType.Text);
        }

        public OperationStatus<IList<Outbox>> GetAllPendingMessages()
        {
            return SelectList<Outbox>(QueryOutbox.GET_ALL_PENDING, CommandType.Text);
        }

        public OperationStatus AddOutboxStatus(int messageId, bool issuccess)
        {
            AddParameter(new SQLiteParameter(@"@IsSuccess", DbType.Boolean), issuccess);
            AddParameter(new SQLiteParameter(@"@MessageId", DbType.Int32), messageId);
            return InsertUpdateItem(QueryOutbox.INSERT_OUTBOX_STATUS, CommandType.Text);
        }
    }
}