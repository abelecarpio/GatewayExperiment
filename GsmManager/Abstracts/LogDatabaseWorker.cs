using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using SqliteDatabaseManager;

namespace GsmManager.Abstracts
{
    public abstract class LogDatabaseWorker : DatabaseWorker
    {
        private const string DATABASE_FILENAME = @"Logs.db";
        private const string DATABASE_PASSWORD = "LogT3l1g3nt$yst3ms";


        protected LogDatabaseWorker()
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
        }
        
        private static string SchemaSystemSetting
        {
            get
            {
                return @"CREATE TABLE 'SystemSetting'('WebApiEnable' Boolean NOT NULL,'WebApiPort' Integer NOT NULL,'ReceivedCallback' Text,'SentCallback' Text,'FailedCallback' Text );";
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