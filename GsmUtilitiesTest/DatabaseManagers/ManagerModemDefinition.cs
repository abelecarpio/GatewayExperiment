using GsmManager.Entities.GsmEntities;
using GsmUtilitiesTest.Abstracts;
using GsmUtilitiesTest.Queries;
using SqliteDatabaseManager;
using System.Data;
using System.Data.SQLite;

namespace GsmUtilitiesTest.DatabaseManagers
{
    internal class ManagerModemDefinition : GsmDatabaseWorker
    {
        internal OperationStatus<ModemDefinition> GetModemDefinition()
        {
            return SelectSingle<ModemDefinition>(QueryModemDefinition.GET_MODEM_DEFINITION, CommandType.Text);
        }

        internal OperationStatus SaveModemConfiguration(ModemDefinition modem)
        {
            var result = SelectSingleGeneric<int>(QueryModemDefinition.CHECK_IF_HAS_MODEM, CommandType.Text);
            if (!result.IsSuccess) throw result.CurrentException;

            AddParameter(new SQLiteParameter(@"@AutoConnect", DbType.Boolean), modem.AutoConnect);
            AddParameter(new SQLiteParameter(@"@BaudRate", DbType.Int32), modem.BaudRate);
            AddParameter(new SQLiteParameter(@"@ComPort", DbType.String), modem.ComPort);
            AddParameter(new SQLiteParameter(@"@Imei", DbType.String), modem.Imei);
            AddParameter(new SQLiteParameter(@"@LogFailed", DbType.Boolean), modem.LogFailed);
            AddParameter(new SQLiteParameter(@"@LogReceived", DbType.Boolean), modem.LogReceived);
            AddParameter(new SQLiteParameter(@"@LogSent", DbType.Boolean), modem.LogSent);
            AddParameter(new SQLiteParameter(@"@ModemName", DbType.String), modem.ModemName);
            AddParameter(new SQLiteParameter(@"@RetryAttempt", DbType.Int32), modem.RetryAttempt);
            AddParameter(new SQLiteParameter(@"@SendingInterval", DbType.Int32), modem.SendingInterval);
            AddParameter(new SQLiteParameter(@"@SendingTimeout", DbType.Int32), modem.SendingTimeout);

            if (result.OperationResult > 0)
                AddParameter(new SQLiteParameter(@"@ModemId", DbType.Int32), modem.ModemId);

            return (result.OperationResult > 0)
                ? InsertUpdateItem(QueryModemDefinition.UPDATE_MODEM_DEFINITION, CommandType.Text)
                : InsertUpdateItem(QueryModemDefinition.INSERT_MODEM_DEFINITION, CommandType.Text);
        }
    }
}