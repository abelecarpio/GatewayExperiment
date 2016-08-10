using GsmManager.Abstracts;
using GsmManager.Entities.GsmEntities;
using GsmManager.Queries;
using SqliteDatabaseManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace GsmManager.DataManagers.GsmDataManagers
{
    public class ManagerKeywordBuilder : GsmDatabaseWorker
    {
        public OperationStatus<IList<KeywordBuilder>> SelectAllKeyword()
        {
            return SelectList<KeywordBuilder>(QueryKeywordBuilder.GET_ALL_KEYWORD, CommandType.Text);
        }

        public bool CheckIfKeywordExist(string keyword)
        {
            bool returnVal = false;

            AddParameter(new SQLiteParameter(@"@Keyword", DbType.String), keyword);

            var result = SelectSingleGeneric<int>(QueryKeywordBuilder.CHECK_KEYWORD_EXIST, CommandType.Text);

            if (!result.IsSuccess) throw result.CurrentException;

            if (result.OperationResult != null && result.OperationResult > 0) returnVal = true;

            return returnVal;
        }

        public bool CheckIfKeywordExistWithID(string keyword, Int16 id)
        {
            bool returnVal = false;

            AddParameter(new SQLiteParameter(@"@Keyword", DbType.String), keyword);
            AddParameter(new SQLiteParameter(@"@KeywordId", DbType.Int16), id);

            var result = SelectSingleGeneric<int>(QueryKeywordBuilder.CHECK_KEYWORD_EXIST_WITH_ID, CommandType.Text);

            if (!result.IsSuccess) throw result.CurrentException;

            if (result.OperationResult != null && result.OperationResult > 0) returnVal = true;

            return returnVal;
        }

        public OperationStatus InsertKeyword(KeywordBuilder model)
        {
            string status = string.Empty;
            string callback = string.Empty;

            if (model.IsActive) status = "1";
            else status = "0";

            if (model.EnableCallback) callback = "1";
            else callback = "0";

            AddParameter(new SQLiteParameter(@"@Keyword", DbType.String), model.Keyword);
            AddParameter(new SQLiteParameter(@"@Spiel", DbType.String), model.Spiel);
            AddParameter(new SQLiteParameter(@"@IsActive", DbType.String), status);
            AddParameter(new SQLiteParameter(@"@EnableCallback", DbType.String), callback);
            AddParameter(new SQLiteParameter(@"@ModemId", DbType.Int16), 0);

            return InsertUpdateItem(QueryKeywordBuilder.INSERT_KEYWORD, CommandType.Text);
        }

        public OperationStatus UpdateKeyword(KeywordBuilder model)
        {
            string status = string.Empty;
            string callback = string.Empty;

            if (model.IsActive) status = "1";
            else status = "0";

            if (model.EnableCallback) callback = "1";
            else callback = "0";

            AddParameter(new SQLiteParameter(@"@KeywordId", DbType.Int16), model.KeywordId);
            AddParameter(new SQLiteParameter(@"@Keyword", DbType.String), model.Keyword);
            AddParameter(new SQLiteParameter(@"@Spiel", DbType.String), model.Spiel);
            AddParameter(new SQLiteParameter(@"@IsActive", DbType.String), status);
            AddParameter(new SQLiteParameter(@"@EnableCallback", DbType.String), callback);
            AddParameter(new SQLiteParameter(@"@ModemId", DbType.Int16), 0);

            return InsertUpdateItem(QueryKeywordBuilder.UPDATE_KEYWORD, CommandType.Text);
        }

        public OperationStatus DeleteKeyword(string keyword, Int16 id)
        {
            AddParameter(new SQLiteParameter(@"@Keyword", DbType.String), keyword);
            AddParameter(new SQLiteParameter(@"@KeywordID", DbType.Int16), id);

            return InsertUpdateItem(QueryKeywordBuilder.DELETE_KEYWORD, CommandType.Text);
        }
    }
}