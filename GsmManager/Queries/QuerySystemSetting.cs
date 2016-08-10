namespace GsmManager.Queries
{
    internal static class QuerySystemSetting
    {
        internal const string GET_SYSTEM_SETTING = @"SELECT FailedCallback, ReceivedCallback, SentCallback, WebApiEnable, WebApiPort FROM SystemSetting LIMIT 1;";

        internal const string INSERT_SYSTEM_SETTING = @"INSERT INTO SystemSetting (ReceivedCallback, SentCallback, FailedCallback, WebApiEnable, WebApiPort) 
                                                      VALUES (@Received, @Sent, @Failed, @EnableAPI, @WebApiPort);";

        internal const string UPDATE_SYSTEM_SETTING = @"UPDATE SystemSetting SET ReceivedCallback = @Received, SentCallback = @Sent, FailedCallback = @Failed, WebApiEnable = @EnableAPI;";

        internal const string CHECK_API_VALUE = @"SELECT count(*) FROM SystemSetting WHERE WebApiEnable = '1';";
    }
}