using GsmManager.Abstracts;
using GsmManager.Entities.GsmEntities;
using GsmManager.Queries;
using SqliteDatabaseManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace GsmManager.DataManagers.GsmDataManagers
{
    public class ManagerMobilePattern : GsmDatabaseWorker
    {
        public OperationStatus<IList<MobilePattern>> SelectAllPattern()
        { 
            return SelectList<MobilePattern>(QueryMobilePattern.GET_ALL_PATTERN, CommandType.Text);
        }

        public bool CheckIfPatternExist(string pattern)
        {
            bool returnVal = false;

            AddParameter(new SQLiteParameter(@"@Pattern", DbType.String), pattern);
            
            var result = SelectList<MobilePattern>(QueryMobilePattern.CHECK_PATTERN_EXIST, CommandType.Text);

            if (!result.IsSuccess) throw result.CurrentException;

            if (result.OperationResult != null && result.OperationResult.Count > 0) returnVal = true;

            return returnVal;
        }

        public bool CheckIfPatternExistWithID(string pattern, long id)
        {
            bool returnVal = false;

            AddParameter(new SQLiteParameter(@"@Pattern", DbType.String), pattern);
            AddParameter(new SQLiteParameter(@"@PatternId", DbType.Int16), id);

            var result = SelectSingleGeneric<int>(QueryMobilePattern.CHECK_PATTERN_EXIST_WITH_ID, CommandType.Text);

            if (!result.IsSuccess) throw result.CurrentException;

            if (result.OperationResult != null && result.OperationResult > 0) returnVal = true;

            return returnVal;
        }

        public OperationStatus InsertMobilePattern(MobilePattern model)
        {
            AddParameter(new SQLiteParameter(@"@Pattern", DbType.String), model.Pattern);
            AddParameter(new SQLiteParameter(@"@ModemId", DbType.Int16), 0);

            return InsertUpdateItem(QueryMobilePattern.INSERT_PATTERN, CommandType.Text);
        }

        public OperationStatus UpdateMobilePattern(MobilePattern model)
        {
            AddParameter(new SQLiteParameter(@"@Pattern", DbType.String), model.Pattern);
            AddParameter(new SQLiteParameter(@"@PatternId", DbType.Int16), model.PatternId);
            AddParameter(new SQLiteParameter(@"@ModemId", DbType.Int16), 0);

            return InsertUpdateItem(QueryMobilePattern.UPDATE_PATTERN, CommandType.Text);
        }

        public OperationStatus DeletePattern(string pattern, Int16 id)
        {
            AddParameter(new SQLiteParameter(@"@Pattern", DbType.String), pattern);
            AddParameter(new SQLiteParameter(@"@PatternId", DbType.Int16), id);

            return InsertUpdateItem(QueryMobilePattern.DELETE_PATTERN, CommandType.Text);
        }
    }
}
