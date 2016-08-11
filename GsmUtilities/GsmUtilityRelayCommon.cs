using System;
using System.Collections.Concurrent;
using GsmUtilities.Helpers;
using GsmUtilities.Models;

namespace GsmUtilities
{
    public partial class GsmUtilityRelay
    {
        #region PUBLIC CALLBACKS

        public Action<string> OnActivityChanged;

        private void NotifyActivityChanged(string message)
        {
            if (message != null) message = message.Trim();
            CommunicationLogHelper.LogInformation(message);
            if (OnActivityChanged == null) return;
            OnActivityChanged.BeginInvoke(message, null, null);
        }

        public Action OnModemRefreshDone { get; set; }

        private void NotifyOnModemRefreshDone()
        {
            if (OnModemRefreshDone == null) return;
            OnModemRefreshDone.BeginInvoke(null, null);
        }

        public Action<ModemStatus> OnModemStatusChanged { get; set; }

        private void NotifyOnModemStatusChanged(ModemStatus status)
        {
            if (OnModemStatusChanged == null) return;
            OnModemStatusChanged.BeginInvoke(status, null, null);
        }

        public Action OnReceivedQueueChanged { get; set; }

        private void NotifyOnReceivedQueueChanged()
        {
            if (OnReceivedQueueChanged == null) return;
            OnReceivedQueueChanged.BeginInvoke(null, null);
        }

        public Action OnSendQueueChanged { get; set; }

        private void NotifyOnSendQueueChanged()
        {
            if (OnSendQueueChanged == null) return;
            OnSendQueueChanged.BeginInvoke(null, null);
        }

        public Action<SignalStrength> OnSignalStrengthChanged { get; set; }

        private void NotifyOnSignalStrengthChanged(SignalStrength signal)
        {
            if (OnSignalStrengthChanged == null) return;
            OnSignalStrengthChanged.BeginInvoke(signal, null, null);
        }


        public Action OnProcessStateChanged { get; set; }
        private void NotifyOnProcessStateChanged()
        {
            if (OnProcessStateChanged == null) return;
            OnProcessStateChanged.BeginInvoke(null, null);
        }
        #endregion PUBLIC CALLBACKS
        
        #region CONSTRUCTORS

        private GsmUtilityRelay()
        {
            log4net.Config.XmlConfigurator.Configure();
            CanExecute = false;
            LocalInboxMessages = new ConcurrentDictionary<string, SmsMessage>();
            LocalOutboxMessages = new ConcurrentDictionary<string, SmsMessage>();
            LocalAvailableModem = new ConcurrentBag<ModemPreference>();
            BindDeviceDetection();
        }

        #endregion CONSTRUCTORS

        #region SINGLETON DEFINITION

        private static GsmUtilityRelay _instance = null;
        private static readonly object Padlock = new object();

        public static GsmUtilityRelay Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Padlock)
                {
                    if (_instance != null) return _instance;
                    var newValue = new GsmUtilityRelay();
                    System.Threading.Thread.MemoryBarrier();
                    _instance = newValue;
                }
                return _instance;
            }
        }

        #endregion SINGLETON DEFINITION
    }
}