using SqliteDatabaseManager;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

namespace GsmManager.Abstracts
{
    public abstract class SmsDatabaseWorker : DatabaseWorker
    {
        private const string DATABASE_FILENAME = @"Messages.db";
        private const string DATABASE_PASSWORD = "T3l1g3nt$yst3ms";

        protected SmsDatabaseWorker(): base(Path.Combine(DatabaseFolder, DATABASE_FILENAME), DATABASE_PASSWORD)
        {
            var databaseLocation = Path.Combine(DatabaseFolder, DATABASE_FILENAME);
            if (File.Exists(databaseLocation)) return;

            BuildDatabaseSchema();

            var creationResult = InsertUpdateItem(DatabaseSchema, CommandType.Text);
            if (!creationResult.IsSuccess) throw creationResult.CurrentException;
        }

        private void BuildDatabaseSchema()
        {
            LocalSchema = new StringBuilder();
            LocalSchema.AppendLine(SchemaInbox);
            LocalSchema.AppendLine(SchemaOutbox);
            LocalSchema.AppendLine(SchemaOutboxStatus);
        }


        private static string SchemaInbox
        {
            get { return @"CREATE TABLE 'Inbox'( 'MessageId' Integer NOT NULL PRIMARY KEY AUTOINCREMENT, 'MobileNumber' Text NOT NULL, 'SmsMessage' Text NOT NULL, 'ReceivedOn' DateTime NOT NULL, 'RetrievedOn' DateTime NOT NULL, 'IsRead' Boolean NOT NULL );  CREATE INDEX 'nci_inbox_mobilenumber' ON 'Inbox'( 'MobileNumber' );    CREATE INDEX 'nci_inbox_receivedon' ON 'Inbox'( 'ReceivedOn' DESC ); "; }
        }

        private static string SchemaOutbox
        {
            get { return @"CREATE TABLE 'Outbox'( 'MessageId' Integer NOT NULL PRIMARY KEY AUTOINCREMENT, 'MobileNumber' Text NOT NULL, 'SmsMessage' Text NOT NULL, 'CreatedOn' DateTime NOT NULL, 'IsUrgent' Boolean NOT NULL );  CREATE INDEX 'nci_outbox_createdon' ON 'Outbox'( 'CreatedOn' DESC );  CREATE INDEX 'nci_outbox_isurgent' ON 'Outbox'( 'IsUrgent' );  CREATE INDEX 'nci_outbox_mobilenumber' ON 'Outbox'( 'MobileNumber' ); "; }
        }

        private static string SchemaOutboxStatus
        {
            get { return @"CREATE TABLE 'OutboxStatus'( 'MessageId' Integer NOT NULL, 'IsSuccess' Boolean NOT NULL, 'SentOn' DateTime NOT NULL );  CREATE INDEX 'nci_outboxstatus_issuccess' ON 'OutboxStatus'( 'IsSuccess' );  CREATE INDEX 'nci_outboxstatus_messageid' ON 'OutboxStatus'( 'MessageId' );  CREATE INDEX 'nci_outboxstatus_senton' ON 'OutboxStatus'( 'SentOn' DESC ); "; }
        }


        #region PROPERTIES

        private static readonly string DatabaseFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        private StringBuilder LocalSchema { get; set; }
        private string DatabaseSchema { get { return LocalSchema.ToString(); } }

        #endregion
    }
}