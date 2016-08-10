using System;
using System.Data;
using System.Data.SQLite;
using GsmManager.Abstracts;
using GsmManager.Queries;
using SqliteDatabaseManager;

namespace GsmManager.DataManagers.SmsDataManagers
{
    public class ManagerInbox : SmsDatabaseWorker
    {

        public OperationStatus AddInbox(string mobileNumber, string message, DateTime receivedOn)
        {
            AddParameter(new SQLiteParameter(@"@MobileNumber", DbType.String), mobileNumber);
            AddParameter(new SQLiteParameter(@"@SmsMessage", DbType.String), message);
            AddParameter(new SQLiteParameter(@"@ReceivedOn", DbType.String), receivedOn);
            return InsertUpdateItem(QueryInbox.INSERT_INBOX, CommandType.Text);
        }
    }
}