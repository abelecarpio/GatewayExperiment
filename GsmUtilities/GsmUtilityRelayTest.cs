using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers;
using GsmUtilities.Models;
using GsmUtilities.Operations;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace GsmUtilities
{
    public sealed class GsmUtilityRelayTest
    {
        #region CONSTRUCTORS

        private GsmUtilityRelayTest()
        {
            RefresherQueue = new ConcurrentQueue<Action>();
            ModemRefresher = new BackgroundWorker();
            ModemRefresher.DoWork += ModemRefresherOnDoWork;
            ModemRefresher.RunWorkerCompleted += ModemRefresherOnRunWorkerCompleted;
            ModemRefresher.RunWorkerAsync();

            BindDeviceDetection();
            GatewayRoutine.Instance.OnSignalChanged = OnModemSignalChanged;
            GatewayRoutine.Instance.SetModem(null);
            LocalAvailableModem = new ConcurrentBag<ModemPreference>();
            LocalProcessBinding = ProcessBinding.ComPort;
            SelectedModem = new ModemPreference();
            ActiveSystemSetting = new SystemSetting();
        }

        private void ModemRefresherOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            ModemRefresher.DoWork -= ModemRefresherOnDoWork;
            ModemRefresher.RunWorkerCompleted -= ModemRefresherOnRunWorkerCompleted;
            ModemRefresher = null;
            ErrorLogHelper<GsmUtilityRelayTest>.LogError(new Exception("Modem Refresher has been Stoped."));
        }

        private void ModemRefresherOnDoWork(object sender, DoWorkEventArgs args)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    if (RefresherQueue == null || RefresherQueue.IsEmpty || !_canRefresh) continue;
                    Action actionqueued;
                    if (!RefresherQueue.TryDequeue(out actionqueued)) continue;
                    if (actionqueued == null) continue;
                    _canRefresh = false;
                    actionqueued.BeginInvoke(null, null);
                }
                catch (Exception ex) { ErrorLogHelper<GsmUtilityRelayTest>.LogError(ex); }
            }
        }

        #endregion CONSTRUCTORS

        #region PUBLIC PROPERTIES

        public bool IsUtilityReady { get { return _isRelayReady; } }
        public ConcurrentBag<ModemPreference> AvailableModem { get { return LocalAvailableModem; } }
        public ConcurrentDictionary<string, SmsMessage> InboxMessages { get { return GatewayRoutine.Instance.SendingQueues; } }
        public ConcurrentDictionary<string, SmsMessage> OutboxMessages { get { return GatewayRoutine.Instance.ReceivedQueues; } }
        public ProcessState CurrentProcessState { get { return GatewayRoutine.Instance.CurrentProcessState; } }

        #endregion PUBLIC PROPERTIES

        #region PUBLIC FUNCTIONS

        public void Activate(ProcessBinding bindProcessed = ProcessBinding.ComPort)
        {
            LocalProcessBinding = bindProcessed;
            CommunicationLogHelper.LogInformation("GSM Relay Activated.\r\n");
            RefresherQueue.Enqueue(InitialRefreshModemList);
        }

        public void RefreshModemList()
        {
            RefresherQueue.Enqueue(SubRefreshModemList);
        }

        private void SubRefreshModemList()
        {
            _stateUsedLogic = (_stateUsedLogic != StateUsedLogic.UserSwitchOff)
               ? StateUsedLogic.UserRefreshModem
               : _stateUsedLogic;
            GatewayRoutine.Instance.StopRoutine();
            GatewayRoutine.Instance.SetModem(null);
            WaitForProcessToStopped();
            var localModemOps = new ModemOperation
            {
                OnModemListingOperationCompleted = delegate(object obj)
                {
                    LocalAvailableModem = new ConcurrentBag<ModemPreference>();
                    LocalAvailableModem = obj as ConcurrentBag<ModemPreference>;
                    GetAndSetCommonSetting();
                    InvokeActivityChanged("");
                    InvokeModemRefreshDone();
                },
                OnActivityChanged = InvokeActivityChanged
            };
            localModemOps.GetAvailableModem();
        }

        public void UpdateSettings()
        {
            _isRelayReady = false;
            GatewayRoutine.Instance.StopRoutine();
            GatewayRoutine.Instance.SetModem(null);
            WaitForProcessToStopped();
            GetAndSetCommonSetting();
            _isRelayReady = true;
        }

        public void StartProcess()
        {
            if (ActiveSystemSetting == null)
                throw new Exception("System Setting is undefined, unable to start this time, please try again.");
            if (SelectedModem == null)
                throw new Exception("Modem is undefined, unable to start this time, please try again.");

            _stateUsedLogic = StateUsedLogic.UserSwitchOn;
            GatewayRoutine.Instance.SetModem(SelectedModem);
            GatewayRoutine.Instance.StartRoutine();
        }

        public void StopProcess()
        {
            _stateUsedLogic = StateUsedLogic.UserSwitchOff;
            GatewayRoutine.Instance.StopRoutine();
            GatewayRoutine.Instance.SetModem(null);
        }

        public void SendMessage(SmsMessage message)
        {
            GatewayRoutine.Instance.SendMessage(message);
        }

        #endregion PUBLIC FUNCTIONS

        #region PRIVATE PROPERTIES

        private volatile bool _canRefresh = true;
        private volatile bool _isRelayReady = false;

        private StateUsedLogic _stateUsedLogic = StateUsedLogic.SystemPreference;

        private ConcurrentBag<ModemPreference> LocalAvailableModem { get; set; }
        private ProcessBinding LocalProcessBinding { get; set; }

        private ModemPreference ShadowSelectedModem { get; set; }

        private ModemPreference SelectedModem
        {
            get { return ShadowSelectedModem; }
            set
            {
                ShadowSelectedModem = value;
                InvokeModemStatusChanged((value == null) ? ModemStatus.Disconnected : ModemStatus.Connected);
            }
        }

        private SystemSetting ActiveSystemSetting { get; set; }

        private ModemDefinition ActiveModemDefinition { get; set; }

        private ConcurrentQueue<Action> RefresherQueue { get; set; }
        private BackgroundWorker ModemRefresher { get; set; }

        #endregion PRIVATE PROPERTIES

        #region PRIVATE ACTION INVOKERS

        private void InvokeActivityChanged(string activity)
        {
            activity = activity.Replace("\r", " ");
            activity = activity.Replace("\n", " ");
            if (OnActivityChanged == null) return;
            OnActivityChanged.BeginInvoke(activity, null, null);
        }

        private void InvokeModemRefreshDone()
        {
            _canRefresh = true;
            if (OnModemRefreshDone == null) return;
            OnModemRefreshDone.BeginInvoke(null, null);
        }

        private void InvokeModemStatusChanged(ModemStatus newStatus)
        {
            if (OnModemStatusChanged == null) return;
            OnModemStatusChanged.BeginInvoke(newStatus, null, null);
        }

        private void InvokeReceivedQueueChanged()
        {
            if (OnReceivedQueueChanged == null) return;
            OnReceivedQueueChanged.BeginInvoke(null, null);
        }

        private void InvokeSendQueueChanged()
        {
            if (OnSendQueueChanged == null) return;
            OnSendQueueChanged.BeginInvoke(null, null);
        }

        #endregion PRIVATE ACTION INVOKERS

        #region PRIVATE FUNCTIONS

        private void BindDeviceDetection()
        {
            Operations.DeviceDetection.PortArrived += DeviceDetectionOnPortArrived;
            Operations.DeviceDetection.PortRemoveCompleted += DeviceDetectionOnPortRemoveCompleted;
        }

        private void DeviceDetectionOnPortArrived(object sender, PortChangeEventArgs args)
        {
            RefresherQueue.Enqueue(AutoModemRefresh);
        }

        private void DeviceDetectionOnPortRemoveCompleted(object sender, PortChangeEventArgs args)
        {
            RefresherQueue.Enqueue(AutoModemRefresh);
        }

        private void AutoModemRefresh()
        {
            _stateUsedLogic = (_stateUsedLogic != StateUsedLogic.UserSwitchOff)
                ? StateUsedLogic.UserRefreshModem
                : _stateUsedLogic;
            GatewayRoutine.Instance.StopRoutine();
            GatewayRoutine.Instance.SetModem(null);
            WaitForProcessToStopped();
            var localModemOps = new ModemOperation
             {
                 OnModemListingOperationCompleted = delegate(object obj)
                 {
                     LocalAvailableModem = new ConcurrentBag<ModemPreference>();
                     LocalAvailableModem = obj as ConcurrentBag<ModemPreference>;
                     GetAndSetCommonSetting();
                     InvokeActivityChanged("");
                     InvokeModemRefreshDone();
                 },
                 OnActivityChanged = InvokeActivityChanged
             };
            localModemOps.GetAvailableModem();
        }

        private void GetSelectedModem()
        {
            var dbops = new GsmDatabaseOperation();
            ActiveModemDefinition = dbops.GetDefinedComPort();
            GatewayRoutine.Instance.SetModemDefinition(ActiveModemDefinition);
        }

        private void SetSelectedModem()
        {
            if (ActiveModemDefinition == null || string.IsNullOrEmpty(ActiveModemDefinition.ComPort)) return;
            SelectedModem = AvailableModem.FirstOrDefault(
                x => string.Equals(x.ComPort, ActiveModemDefinition.ComPort,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        private void GetSystemSetting()
        {
            var dbops = new GsmDatabaseOperation();
            ActiveSystemSetting = dbops.GetSystemSetting();
            GatewayRoutine.Instance.SetSetting(ActiveSystemSetting);
        }

        private void GetAndSetCommonSetting()
        {
            GetSelectedModem();
            SetSelectedModem();
            GetSystemSetting();

            if (ActiveModemDefinition == null || string.IsNullOrEmpty(ActiveModemDefinition.ComPort))
                _stateUsedLogic = StateUsedLogic.UserSwitchOff;
            if (ActiveSystemSetting == null || SelectedModem == null) return;
            if (ActiveModemDefinition == null || !ActiveModemDefinition.AutoConnect) return;

            if (_stateUsedLogic == StateUsedLogic.UserSwitchOff) return;
            GatewayRoutine.Instance.SetModem(SelectedModem);
            GatewayRoutine.Instance.StartRoutine();
        }

        private void InitialRefreshModemList()
        {
            GatewayRoutine.Instance.StopRoutine();
            GatewayRoutine.Instance.SetModem(null);
            WaitForProcessToStopped();
            var localModemOps = new ModemOperation
             {
                 OnModemListingOperationCompleted = delegate(object obj)
                 {
                     LocalAvailableModem = obj as ConcurrentBag<ModemPreference>;
                     GetAndSetCommonSetting();
                     _isRelayReady = true;
                     InvokeActivityChanged("");
                     InvokeModemRefreshDone();
                 }
             };
            localModemOps.GetAvailableModem();
        }

        private static void WaitForProcessToStopped()
        {
            try
            {
                while (GatewayRoutine.Instance.CurrentProcessState != ProcessState.Stopped) { Thread.Sleep(10); }
            }
            catch (Exception ex) { ErrorLogHelper<GsmUtilityRelayTest>.LogError(ex); }
        }

        #endregion PRIVATE FUNCTIONS

        #region PUBLIC EVENTS

        #region EXPOSED

        public Action<string> OnActivityChanged
        {
            get { return LocalOnActivityChanged; }
            set
            {
                LocalOnActivityChanged = value;
                GatewayRoutine.Instance.OnActivityChanged = value;
            }
        }

        public Action OnModemRefreshDone { get; set; }
        public Action<ModemStatus> OnModemStatusChanged { get; set; }
        public Action OnReceivedQueueChanged { get; set; }
        public Action OnSendQueueChanged { get; set; }

        public Action<SignalStrength> OnSignalStrengthChanged { get; set; }

        public Action OnProcessStateChanged
        {
            get { return GatewayRoutine.Instance.OnProcessStateChanged; }
            set { GatewayRoutine.Instance.OnProcessStateChanged = value; }
        }


        private SignalStrength LocalModemSignalStrength { get; set; }
        public SignalStrength ModemSignalStrength { get { return LocalModemSignalStrength; } }

        #endregion EXPOSED

        #region PRIVATE EVENTS

        private Action<string> LocalOnActivityChanged { get; set; }
        

        private void OnModemSignalChanged(SignalStrength value)
        {
            LocalModemSignalStrength = value;
            if (OnSignalStrengthChanged == null) return;
            OnSignalStrengthChanged.BeginInvoke(value, null, null);
        }


        #endregion PRIVATE EVENTS

        #endregion PUBLIC EVENTS

        #region SINGLETON DEFINITION

        private static GsmUtilityRelayTest _instance = null;
        private static readonly object Padlock = new object();

        public static GsmUtilityRelayTest Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Padlock)
                {
                    if (_instance != null) return _instance;
                    var newValue = new GsmUtilityRelayTest();
                    System.Threading.Thread.MemoryBarrier();
                    _instance = newValue;
                }
                return _instance;
            }
        }

        #endregion SINGLETON DEFINITION
    }
}