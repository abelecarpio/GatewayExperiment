using System.Data;
using System.Data.SQLite;
using GsmManager.Entities.GsmEntities;
using GsmUtilitiesTest.Abstracts;
using GsmUtilitiesTest.Queries;
using SqliteDatabaseManager;

namespace GsmUtilitiesTest.DatabaseManagers
{
    internal class ManagerSystemSetting : GsmDatabaseWorker
    {
        public OperationStatus<SystemSetting> GetSystemSetting()
        {
            return SelectSingle<SystemSetting>(QuerySystemSetting.GET_SYSTEM_SETTING, CommandType.Text);
        }
        
        public OperationStatus SaveSystemSettings(SystemSetting setting)
        {
            AddParameter(new SQLiteParameter(@"@FailedCallback", DbType.String), setting.FailedCallback);
            AddParameter(new SQLiteParameter(@"@ReceivedCallback", DbType.String), setting.ReceivedCallback);
            AddParameter(new SQLiteParameter(@"@SentCallback", DbType.String), setting.SentCallback);
            AddParameter(new SQLiteParameter(@"@WebApiEnable", DbType.Boolean), setting.WebApiEnable);
            AddParameter(new SQLiteParameter(@"@WebApiPort", DbType.Int32), setting.WebApiPort);
            
            return InsertUpdateItem(QuerySystemSetting.UPDATE_SYSTEM_SETTING, CommandType.Text);
        }
    }
}