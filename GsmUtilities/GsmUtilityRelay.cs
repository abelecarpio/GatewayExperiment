using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers;
using GsmUtilities.Models;
using GsmUtilities.Operations;

namespace GsmUtilities
{
    public partial class GsmUtilityRelay
    {

        #region PUBLIC PROPERTIES

        public bool RelayIsReady { get { return ShouldResume && IsInitialized; } }
        public SignalStrength ModemSignalStrength
        {
            get
            {
                if (!RelayIsReady)
                    throw new MethodAccessException("Utility Relay is not yet ready, try again later.");

                if (RoutineOps == null) return SignalStrength.Offline;
                switch (RoutineOps.SignalState)
                {
                    case InternalSignalStrength.None:
                        return SignalStrength.Offline;

                    case InternalSignalStrength.Offline:
                        return SignalStrength.Offline;

                    case InternalSignalStrength.Low:
                        return SignalStrength.Low;

                    case InternalSignalStrength.Fair:
                        return SignalStrength.Fair;

                    case InternalSignalStrength.Good:
                        return SignalStrength.Good;

                    case InternalSignalStrength.Excellent:
                        return SignalStrength.Excellent;

                    default:
                        return SignalStrength.Offline;
                }
            }
        }

        private ConcurrentBag<ModemPreference> LocalAvailableModem { get; set; }
        public ConcurrentBag<ModemPreference> AvailableModem { get { return LocalAvailableModem; } }

        private ConcurrentDictionary<string, SmsMessage> LocalInboxMessages { get; set; }
        public ConcurrentDictionary<string, SmsMessage> InboxMessages { get { return LocalInboxMessages; } }

        private ConcurrentDictionary<string, SmsMessage> LocalOutboxMessages { get; set; }
        public ConcurrentDictionary<string, SmsMessage> OutboxMessages { get { return LocalOutboxMessages; } }


        public ProcessState CurrentProcessState
        {
            get { return (RoutineOps != null && RoutineOps.IsRoutineRunning) ? ProcessState.Running : ProcessState.Stopped; }
        }

        #endregion

        #region PUBLIC FUNCTIONS

        public void Activate()
        {
            if (IsActivated) throw new AccessViolationException("Relay is already activated.");
            CommunicationLogHelper.LogInformation("GSM Relay Activated.\r\n");
            IsActivated = true;
            var woker = new BackgroundWorker();
            woker.DoWork += (sender, args) =>
            {
                GetSystemSetting();
                GetdModemDefinition();
                GetPendingMessages();
            };
            woker.RunWorkerCompleted += (sender, args) =>
            {
                GetModemList(true);
            };
            woker.RunWorkerAsync();
        }

        public void RefreshModemList()
        {
            if (!IsInitialized) throw new MethodAccessException("Utility Relay is not yet ready, try again later.");

            if (RoutineOps != null)
            {
                RoutineOps.Stop();
                RoutineOps = null;
            }
            GetModemList();
        }

        public void UpdateSettings(SystemSetting setting, ModemDefinition modem)
        {
            if (setting == null) throw new ArgumentNullException("setting", @"System Setting should not be empty.");
            if (modem == null || string.IsNullOrEmpty(modem.ComPort)) throw new ArgumentNullException("modem", @"Modem Definition should not be empty.");

            try
            {
                ActiveSystemSetting = setting;
                ActiveModemDefinition = modem;
                CanExecute = false;
                if (RoutineOps != null) RoutineOps.Stop();
                RunRoutine();
            }
            catch (Exception ex) { ErrorLogHelper<GsmUtilityRelay>.LogError(ex); }
        }

        public void StartProcess()
        {
            if (!RelayIsReady) throw new MethodAccessException("Utility Relay is not yet ready, try again later.");

            if (ActiveSystemSetting == null) throw new Exception(@"System Setting should not be empty.");
            if (ActiveModemDefinition == null || string.IsNullOrEmpty(ActiveModemDefinition.ComPort)) throw new Exception(@"Modem Definition should not be empty.");

            try
            {
                CanExecute = true;
                RunRoutine();
            }
            catch (Exception ex) { ErrorLogHelper<GsmUtilityRelay>.LogError(ex); }
        }

        public void StopProcess()
        {
            try
            {
                CanExecute = false;
                if (RoutineOps == null) return;
                RoutineOps.Stop();
            }
            catch (Exception ex) { ErrorLogHelper<GsmUtilityRelay>.LogError(ex); }
            finally { RoutineOps = null; }
            NotifyOnProcessStateChanged();
        }

        public void SendMessage(SmsMessage message)
        {
            if (!RelayIsReady)
                throw new MethodAccessException("Utility Relay is not yet ready, try again later.");

        }

        #endregion PUBLIC FUNCTIONS
        
        #region PRIVATE PROPERTIES
        private bool IsInitialized { get; set; }
        private RoutineOperation RoutineOps { get; set; }
        private ModemDefinition ActiveModemDefinition { get; set; }
        private SystemSetting ActiveSystemSetting { get; set; }
        private bool CanExecute { get; set; }
        private bool ShouldResume
        {
            get
            {
                if (ActiveSystemSetting == null) return false;
                if (ActiveModemDefinition == null) return false;
                if (string.IsNullOrEmpty(ActiveModemDefinition.ComPort)) return false;
                if (AvailableModem == null) return false;
                if (AvailableModem.All(x => x.ComPort != ActiveModemDefinition.ComPort)) return false;
                return true;
            }
        }
        private bool IsActivated { get; set; }
        #endregion

        #region PRIVATE FUNCTIONS

        private void GetSystemSetting()
        {
            var dbops = new GsmDatabaseOperation();
            ActiveSystemSetting = dbops.GetSystemSetting();
        }

        private void GetdModemDefinition()
        {
            var dbops = new GsmDatabaseOperation();
            var result = dbops.GetDefinedComPort();
            if (result == null) ActiveModemDefinition = null;
            else if (string.IsNullOrEmpty(result.ComPort)) ActiveModemDefinition = null;
            else ActiveModemDefinition = result;
        }

        private void GetPendingMessages()
        {
            //TODO: GET ALL PENDING MESSAGES
        }
        private void BindDeviceDetection()
        {
           Operations.DeviceDetection.PortArrived += DeviceDetectionOnPortArrived;
           Operations.DeviceDetection.PortRemoveCompleted += DeviceDetectionOnPortRemoveCompleted;
        }

        private void DeviceDetectionOnPortArrived(object sender, PortChangeEventArgs args) { RefreshModemList(); }

        private void DeviceDetectionOnPortRemoveCompleted(object sender, PortChangeEventArgs args) { RefreshModemList(); }

        private void GetModemList(bool isInitial = false)
        {
            var modemops = new ModemOperation
            {
                OnActivityChanged = NotifyActivityChanged,
                OnModemListingOperationCompleted = modems =>
                {
                    LocalAvailableModem = modems as ConcurrentBag<ModemPreference>;
                    NotifyOnModemRefreshDone();
                    if (isInitial)
                        CanExecute = (ActiveModemDefinition != null && !string.IsNullOrEmpty(ActiveModemDefinition.ComPort) && ActiveModemDefinition.AutoConnect);
                    
                    if (ActiveModemDefinition != null && !string.IsNullOrEmpty(ActiveModemDefinition.ComPort) 
                        && AvailableModem.Any(x => x.ComPort == ActiveModemDefinition.ComPort))
                        NotifyOnModemStatusChanged(ModemStatus.Connected);
                    else NotifyOnModemStatusChanged(ModemStatus.Disconnected);

                    RunRoutine();
                    IsInitialized = true;
                }
            };
            modemops.GetAvailableModem();
        }

        private void RunRoutine()
        {
            if (!ShouldResume || !CanExecute) return;
            RoutineOps = new RoutineOperation
            {
                OnActivityChanged = NotifyActivityChanged,
                OnReceivedQueueChanged = NotifyOnReceivedQueueChanged,
                OnSendQueueChanged = NotifyOnSendQueueChanged,
                OnSignalStrengthChanged = NotifyOnSignalStrengthChanged
            };

            RoutineOps.Start(ActiveSystemSetting, ActiveModemDefinition);
            NotifyOnProcessStateChanged();
        }

        #endregion
    }
}