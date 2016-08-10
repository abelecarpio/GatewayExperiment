namespace GsmManager.Queries
{
    internal static class QueryModemDefinition
    {
        internal const string GET_MODEM_DEFINITION = @"SELECT AutoConnect, BaudRate, ComPort, Imei, LogFailed, LogReceived, LogSent, ModemId, ModemName, RetryAttempt, SendingInterval, SendingTimeout FROM ModemDefinition LIMIT 1;";

        internal const string INSERT_MODEM_DEFINITION = @"INSERT INTO ModemDefinition ( AutoConnect, BaudRate, ComPort, Imei, LogFailed, LogReceived, LogSent, ModemName, RetryAttempt, SendingInterval, SendingTimeout) VALUES (@AutoConnect, @BaudRate, @ComPort, @Imei, @LogFailed, @LogReceived, @LogSent, @ModemName, @RetryAttempt, @SendingInterval, @SendingTimeout);";

        internal const string UPDATE_MODEM_DEFINITION = @"UPDATE ModemDefinition SET AutoConnect = @AutoConnect, BaudRate = @BaudRate, ComPort = @ComPort, Imei = @Imei, LogFailed = @LogFailed, LogReceived = @LogReceived, LogSent = @LogSent, ModemName = @ModemName, RetryAttempt = @RetryAttempt, SendingInterval = @SendingInterval, SendingTimeout = @SendingTimeout;";
    }
}