namespace GsmManager.Entities.GsmEntities
{
    public class ModemDefinition
    {
        public int ModemId { get; set; }
        public string ModemName { get; set; }
        public int RetryAttempt { get; set; }
        public int SendingTimeout { get; set; }
        public int SendingInterval { get; set; }
        public bool LogReceived { get; set; }
        public bool LogSent { get; set; }
        public bool LogFailed { get; set; }
        public bool AutoConnect { get; set; }
        public string ComPort { get; set; }
        public string Imei { get; set; }
        public int BaudRate { get; set; }
    }
}