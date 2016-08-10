using System.Data;
using GsmManager.Abstracts;
using GsmManager.Entities.GsmEntities;
using GsmManager.Queries;
using SqliteDatabaseManager;
using System.Data.SQLite;

namespace GsmManager.DataManagers.GsmDataManagers
{
    public class ManagerSystemSetting : GsmDatabaseWorker
    {
        public OperationStatus<SystemSetting> GetSystemSetting()
        {
           return SelectSingle<SystemSetting>(QuerySystemSetting.GET_SYSTEM_SETTING, CommandType.Text);
        }
        public OperationStatus InsertSystemSettings(SystemSetting model)
        {
            string enableAPI = string.Empty;

            if (model.WebApiEnable) enableAPI = "1";
            else enableAPI = "0";

            AddParameter(new SQLiteParameter(@"@Received", DbType.String), model.ReceivedCallback);
            AddParameter(new SQLiteParameter(@"@Sent", DbType.String), model.SentCallback);
            AddParameter(new SQLiteParameter(@"@Failed", DbType.String), model.FailedCallback);
            AddParameter(new SQLiteParameter(@"@EnableAPI", DbType.String), enableAPI);
            AddParameter(new SQLiteParameter(@"@WebApiPort", DbType.Int32), model.WebApiPort);

            return InsertUpdateItem(QuerySystemSetting.INSERT_SYSTEM_SETTING, CommandType.Text);
        }
        public OperationStatus UpdateSystemSettings(SystemSetting model)
        {
            string enableAPI = string.Empty;

            if (model.WebApiEnable) enableAPI = "1";
            else enableAPI = "0";

            AddParameter(new SQLiteParameter(@"@Received", DbType.String), model.ReceivedCallback);
            AddParameter(new SQLiteParameter(@"@Sent", DbType.String), model.SentCallback);
            AddParameter(new SQLiteParameter(@"@Failed", DbType.String), model.FailedCallback);
            AddParameter(new SQLiteParameter(@"@EnableAPI", DbType.String), enableAPI);

            return InsertUpdateItem(QuerySystemSetting.UPDATE_SYSTEM_SETTING, CommandType.Text);
        }

        public bool CheckWebAPIValue()
        {
            bool returnVal = false;

            var result = SelectSingleGeneric<int>(QuerySystemSetting.CHECK_API_VALUE, CommandType.Text);

            if (!result.IsSuccess) throw result.CurrentException;

            if (result.OperationResult != null && result.OperationResult > 0) returnVal = true;

            return returnVal;
        }
    }
}