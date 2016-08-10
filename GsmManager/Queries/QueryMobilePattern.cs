namespace GsmManager.Queries
{
    internal static class QueryMobilePattern
    {
        internal const string GET_ALL_PATTERN = @"SELECT ModemId, Pattern, PatternId FROM WhiteList;";

        internal const string CHECK_PATTERN_EXIST = @"SELECT count(*) FROM WhiteList WHERE Pattern = @Pattern;";

        internal const string CHECK_PATTERN_EXIST_WITH_ID = @"SELECT count(*) FROM WhiteList WHERE Pattern = @Pattern AND PatternId <> @PatternId;";

        internal const string INSERT_PATTERN = @"INSERT INTO WhiteList ( ModemId, Pattern) VALUES (@ModemId, @Pattern);";

        internal const string UPDATE_PATTERN = @"UPDATE WhiteList SET ModemId = @ModemId, Pattern = @Pattern WHERE PatternId = @PatternId";

        internal const string DELETE_PATTERN = @"DELETE FROM WhiteList WHERE Pattern = @Pattern AND PatternId = @PatternId;";
    }
}