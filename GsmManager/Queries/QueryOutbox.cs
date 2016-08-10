using System.CodeDom;

namespace GsmManager.Queries
{
    internal static class QueryOutbox
    {
        internal const string GET_ALL_OUTBOX = @"SELECT MessageId, MobileNumber, SmsMessage, CreatedOn, IsUrgent FROM Outbox;";

        internal const string GET_ALL_PENDING = @"SELECT O.MessageId, O.MobileNumber, O.SmsMessage, O.CreatedOn, O.IsUrgent FROM Outbox O LEFT JOIN OutboxStatus OS on O.MessageId = OS.MessageId WHERE OS.IsSuccess IS NULL;";

        internal const string INSERT_OUTBOX = @"INSERT INTO Outbox ( CreatedOn, IsUrgent, MobileNumber, SmsMessage) SELECT datetime('now','localtime'), @IsUrgent, @MobileNumber, @SmsMessage;";

        internal const string INSERT_OUTBOX_STATUS = @"INSERT INTO OutboxStatus ( IsSuccess, MessageId, SentOn) SELECT @IsSuccess, @MessageId, datetime('now','localtime');";
    }
}