namespace GsmUtilitiesTest.Queries
{
    internal static class QuerySystemSetting
    {
        internal const string GET_SYSTEM_SETTING = @"SELECT FailedCallback, ReceivedCallback, SentCallback, WebApiEnable, WebApiPort FROM SystemSetting LIMIT 1;";

        internal const string UPDATE_SYSTEM_SETTING = @"UPDATE SystemSetting SET FailedCallback = @FailedCallback, ReceivedCallback = @ReceivedCallback, SentCallback = @SentCallback, WebApiEnable = @WebApiEnable, WebApiPort = @WebApiPort;";
    }
}