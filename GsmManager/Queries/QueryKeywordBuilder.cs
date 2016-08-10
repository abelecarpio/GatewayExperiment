namespace GsmManager.Queries
{
    internal static class QueryKeywordBuilder
    {

        internal const string GET_ALL_KEYWORD = @"SELECT EnableCallback, IsActive, Keyword, KeywordId, ModemId, Spiel FROM Keyword;";

        internal const string GET_SOME_KEYWORD = @"SELECT EnableCallback, IsActive, Keyword, KeywordId, ModemId, Spiel FROM Keyword WHERE Keyword Like @Keyword;";

        internal const string CHECK_KEYWORD_EXIST_WITH_ID = @"SELECT count(*) FROM Keyword WHERE Keyword = @Keyword AND KeywordId <> @KeywordId;";

        internal const string CHECK_KEYWORD_EXIST = @"SELECT count(*) FROM Keyword WHERE Keyword = @Keyword";

        internal const string INSERT_KEYWORD = @"INSERT INTO Keyword ( EnableCallback, IsActive, Keyword, ModemId, Spiel) VALUES (@EnableCallback, @IsActive, @Keyword, @ModemId, @Spiel);";

        internal const string UPDATE_KEYWORD = @"UPDATE Keyword SET EnableCallback = @EnableCallback, IsActive = @IsActive, Keyword = @Keyword, ModemId = @ModemId, Spiel = @Spiel WHERE KeywordId = @KeywordId;";

        internal const string DELETE_KEYWORD = @"DELETE FROM Keyword WHERE Keyword = @Keyword AND KeywordId = @KeywordID;";
    }
}