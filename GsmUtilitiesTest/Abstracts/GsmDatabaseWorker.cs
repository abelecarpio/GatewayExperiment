using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using SqliteDatabaseManager;

namespace GsmUtilitiesTest.Abstracts
{
    internal class GsmDatabaseWorker : DatabaseWorker
    {
        private const string DATABASE_FILENAME = @"Configuration.db";
        private const string DATABASE_PASSWORD = "T3l1g3nt$yst3ms";


        protected GsmDatabaseWorker()
            : base(Path.Combine(DatabaseFolder, DATABASE_FILENAME), DATABASE_PASSWORD)
        {
            var databaseLocation = Path.Combine(DatabaseFolder, DATABASE_FILENAME);
            if (File.Exists(databaseLocation)) return;

            BuildDatabaseSchema();
            var creationResult = InsertUpdateItem(DatabaseSchema, CommandType.Text);
            if (!creationResult.IsSuccess) throw creationResult.CurrentException;
        }

        #region DATABASE SCHEMAS

        private void BuildDatabaseSchema()
        {
            LocalSchema = new StringBuilder();
            LocalSchema.AppendLine(SchemaSystemSetting);
            LocalSchema.AppendLine(SchemaModemDefinition);
            LocalSchema.AppendLine(SchemaKeyword);
            LocalSchema.AppendLine(WhiteList);
        }

        private static string SchemaSystemSetting
        {
            get
            {
                return @"CREATE TABLE 'SystemSetting'('WebApiEnable' Boolean NOT NULL,'WebApiPort' Integer NOT NULL,'ReceivedCallback' Text,'SentCallback' Text,'FailedCallback' Text ); INSERT INTO SystemSetting ( FailedCallback, ReceivedCallback, SentCallback, WebApiEnable, WebApiPort) VALUES ( '', '', '', 0,0);";
            }
        }

        private static string SchemaModemDefinition
        {
            get
            {
                return
                    @"CREATE TABLE 'ModemDefinition'( 'ModemId' Integer NOT NULL PRIMARY KEY AUTOINCREMENT, 'ModemName' Text NOT NULL, 'RetryAttempt' Integer NOT NULL, 'SendingTimeout' Integer NOT NULL, 'SendingInterval' Integer NOT NULL, 'LogReceived' Boolean NOT NULL, 'LogSent' Boolean NOT NULL, 'LogFailed' Boolean NOT NULL, 'AutoConnect' Boolean NOT NULL, 'ComPort' Text NOT NULL, 'Imei' Text, 'BaudRate' Integer NOT NULL );";
            }
        }

        private static string SchemaKeyword
        {
            get
            {
                return @"CREATE TABLE 'Keyword'( 'KeywordId' Integer NOT NULL PRIMARY KEY AUTOINCREMENT, 'Keyword' Text NOT NULL, 'Spiel' Text NOT NULL, 'IsActive' Boolean NOT NULL, 'EnableCallback' Boolean NOT NULL, 'ModemId' Integer NOT NULL ); CREATE INDEX 'nci_keyword_modemid' ON 'Keyword'( 'ModemId' );";
            }
        }

        private static string WhiteList
        {
            get
            {
                return @"CREATE TABLE 'WhiteList'( 'PatternId' Integer NOT NULL PRIMARY KEY AUTOINCREMENT, 'ModemId' Integer NOT NULL, 'Pattern' Text NOT NULL ); CREATE INDEX 'nci_whitelist_modemid' ON 'WhiteList'( 'ModemId' );";
            }
        }

        #endregion

        #region PROPERTIES

        private static readonly string DatabaseFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        private StringBuilder LocalSchema { get; set; }
        private string DatabaseSchema { get { return LocalSchema.ToString(); } }

        #endregion
    }
}