namespace GsmManager.Queries
{
    internal static class QueryInbox
    {
        internal const string GET_ALL_INBOX = @"SELECT MessageId, MobileNumber, ReceivedOn, RetrievedOn, SmsMessage, IsRead FROM Inbox;";
        internal const string INSERT_INBOX = @"INSERT INTO Inbox ( MobileNumber, ReceivedOn, RetrievedOn, SmsMessage, IsRead) SELECT @MobileNumber, @ReceivedOn, datetime('now','localtime'), @SmsMessage, '0';";
    }
}