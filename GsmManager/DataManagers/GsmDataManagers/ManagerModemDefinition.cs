using System.Data;
using GsmManager.Abstracts;
using GsmManager.Entities.GsmEntities;
using GsmManager.Queries;
using SqliteDatabaseManager;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace GsmManager.DataManagers.GsmDataManagers
{
    public class ManagerModemDefinition : GsmDatabaseWorker
    {
        public OperationStatus<ModemDefinition> GetModemDefinition()
        {
            return SelectSingle<ModemDefinition>(QueryModemDefinition.GET_MODEM_DEFINITION, CommandType.Text);
        }
        public OperationStatus InsertModemDefinition(ModemDefinition model)
        {
            string received = string.Empty;
            string sent = string.Empty;
            string failed = string.Empty;
            string auto = string.Empty;

            if (model.LogReceived) received = "1";
            else received = "0";

            if (model.LogSent) sent = "1";
            else sent = "0";

            if (model.LogFailed) failed = "1";
            else failed = "0";

            if (model.AutoConnect) auto = "1";
            else auto = "0";

            AddParameter(new SQLiteParameter(@"@ModemName", DbType.String), model.ModemName);
            AddParameter(new SQLiteParameter(@"@RetryAttempt", DbType.Int16), model.RetryAttempt);
            AddParameter(new SQLiteParameter(@"@SendingTimeout", DbType.Int32), model.SendingTimeout);
            AddParameter(new SQLiteParameter(@"@SendingInterval", DbType.Int32), model.SendingInterval);
            AddParameter(new SQLiteParameter(@"@LogReceived", DbType.String), received);
            AddParameter(new SQLiteParameter(@"@LogSent", DbType.String), sent);
            AddParameter(new SQLiteParameter(@"@LogFailed", DbType.String), failed);
            AddParameter(new SQLiteParameter(@"@AutoConnect", DbType.String), auto);
            AddParameter(new SQLiteParameter(@"@Comport", DbType.Int32), 0);
            AddParameter(new SQLiteParameter(@"@BaudRate", DbType.Int32), 0);
            AddParameter(new SQLiteParameter(@"@Imei", DbType.String),"");

            return InsertUpdateItem(QueryModemDefinition.INSERT_MODEM_DEFINITION, CommandType.Text);
        }
        public OperationStatus UpdateModemDefinition(ModemDefinition model)
        {
            string received = string.Empty;
            string sent = string.Empty;
            string failed = string.Empty;
            string auto = string.Empty;

            if (model.LogReceived) received = "1";
            else received = "0";

            if (model.LogSent) sent = "1";
            else sent = "0";

            if (model.LogFailed) failed = "1";
            else failed = "0";

            if (model.AutoConnect) auto = "1";
            else auto = "0";

            AddParameter(new SQLiteParameter(@"@ModemName", DbType.String), model.ModemName);
            AddParameter(new SQLiteParameter(@"@RetryAttempt", DbType.Int16), model.RetryAttempt);
            AddParameter(new SQLiteParameter(@"@SendingTimeout", DbType.Int32), model.SendingTimeout);
            AddParameter(new SQLiteParameter(@"@SendingInterval", DbType.Int32), model.SendingInterval);
            AddParameter(new SQLiteParameter(@"@LogReceived", DbType.String), received);
            AddParameter(new SQLiteParameter(@"@LogSent", DbType.String), sent);
            AddParameter(new SQLiteParameter(@"@LogFailed", DbType.String), failed);
            AddParameter(new SQLiteParameter(@"@AutoConnect", DbType.String), auto);
            AddParameter(new SQLiteParameter(@"@Comport", DbType.Int32), 0);
            AddParameter(new SQLiteParameter(@"@BaudRate", DbType.Int32), 0);
            AddParameter(new SQLiteParameter(@"@Imei", DbType.String), "");

            return InsertUpdateItem(QueryModemDefinition.UPDATE_MODEM_DEFINITION, CommandType.Text);
        }
    }
}