namespace GsmUtilities
{
    public enum MessagePriority : int
    {
        Normal = 0,
        Urgent = 1
    }

    public enum ProcessBinding
    {
        ComPort,
        Imei
    }

    public enum ProcessState
    {
        Running,
        Stopped
    }

    public enum ModemStatus
    {
        Connected,
        Disconnected
    }

    public enum SignalStrength : int
    {
        Offline = 0, // 0, 1 or 99
        Low = 1, //2-9
        Fair = 2, //10-14
        Good = 3, //15-19 
        Excellent = 4 //20 - 98
    }


    internal enum InternalSignalStrength : int
    {
        None = -1,
        Offline = 0, // 0, 1 or 99
        Low = 1, //2-9
        Fair = 2, //10-14
        Good = 3, //15-19 
        Excellent = 4 //20 - 98
    }

    internal enum StateUsedLogic
    {
        SystemPreference,
        UserSwitchOff,
        UserSwitchOn,
        UserRefreshModem
    }

    internal enum MessageStatus
    {
        Sent,
        Received,
        Failed
    }
}