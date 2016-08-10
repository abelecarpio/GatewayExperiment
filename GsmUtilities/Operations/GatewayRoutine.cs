using GsmUtilities.Helpers;
using GsmUtilities.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using GsmManager.DataManagers.SmsDataManagers;
using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers.PduHelpers;

namespace GsmUtilities.Operations
{
    internal class GatewayRoutine
    {
        #region CONTRUCTOR

        private GatewayRoutine()
        {
            ActiveModemPreference = new ConcurrentQueue<ModemPreference>();
            SendingQueues = new ConcurrentDictionary<string, SmsMessage>();
            ReceivedQueues = new ConcurrentDictionary<string, SmsMessage>();
            ProcessWorker = null;
            _canProcess = false;
            ShadowCurrentProcessState = ProcessState.Stopped;
            ProcessWorker = new BackgroundWorker();
            ProcessWorker.DoWork += ProcessWorkerOnDoWork;
            ProcessWorker.RunWorkerCompleted += ProcessWorkerOnRunWorkerCompleted;
            ProcessWorker.RunWorkerAsync();
        }

        #endregion CONTRUCTOR

        #region INTERNAL PROPERTIES

        internal ProcessState CurrentProcessState { get { return LocalCurrentProcessState; } }
        internal ConcurrentDictionary<string, SmsMessage> SendingQueues { get; set; }
        internal ConcurrentDictionary<string, SmsMessage> ReceivedQueues { get; set; }

        #endregion INTERNAL PROPERTIES

        #region INTERNAL ACTIONS

        internal Action<string> OnActivityChanged { get; set; }
        internal Action<SignalStrength> OnSignalChanged { get; set; }
        internal Action OnSentCompleted { get; set; }
        internal Action OnReceivedCompleted { get; set; }
        internal Action OnSentFailed { get; set; }
        internal Action OnProcessStateChanged { get; set; }

        #endregion INTERNAL ACTIONS

        #region INTERNAL FUNCTIONS

        internal void SetSetting(SystemSetting setting) { ActiveSetting = setting; }

        internal void SetModemDefinition(ModemDefinition modem) { ActiveModemDefinition = modem; }

        internal void SetModem(ModemPreference modem) { ActiveModemPreference.Enqueue(modem); }

        internal void StartRoutine()
        {
            _canProcess = true;
        }

        internal void StopRoutine()
        {
            _canProcess = false;
        }

        internal void SendMessage(SmsMessage message)
        {
            try
            {
                if (message == null) return;
                if (string.IsNullOrEmpty(message.MobileNumber)) return;
                SendingQueues.TryAdd(message.MessageId, message);
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
        }

        #endregion INTERNAL FUNCTIONS

        #region PROCESS DEFINITION

        private void ProcessWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            ModemPreference currentModemPreference = null;
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    if (!ActiveModemPreference.IsEmpty)
                        ActiveModemPreference.TryDequeue(out currentModemPreference);

                    LocalCurrentProcessState = (currentModemPreference == null)
                        ? ProcessState.Stopped
                        : ProcessState.Running;

                    if (currentModemPreference == null) continue;

                    //GET SIGNAL
                    var signal = GetSignalStrengthProcess(currentModemPreference);
                    InvokeOnSignalChanged(signal);
                    if (signal == SignalStrength.Offline) continue;

                    SentMessageProcess(currentModemPreference);
                    GetMessageProcess(currentModemPreference);
                }
                catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
            }
        }

        private void ProcessWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            ProcessWorker.DoWork -= ProcessWorkerOnDoWork;
            ProcessWorker.RunWorkerCompleted -= ProcessWorkerOnRunWorkerCompleted;
            ProcessWorker = null;
            ErrorLogHelper<GatewayRoutine>.LogError(new Exception("Routine Process has been Stoped."));
        }

        #endregion PROCESS DEFINITION

        #region SUB PROCESS

        private SignalStrength GetSignalStrengthProcess(ModemPreference modemPreference)
        {
            if (modemPreference == null) return SignalStrength.Offline;
            var modemops = new GsmModemHelper(modemPreference.ComPort);
            return modemops.GetModemSignalStrength();
        }

        private void SentMessageProcess(ModemPreference modemPreference)
        {
            var sendingqueue = new KeyValuePair<string, SmsMessage>();
            try
            {
                if (!_canProcess || modemPreference == null || ActiveSetting == null || ActiveModemDefinition == null ||
                    SendingQueues == null || SendingQueues.IsEmpty) return;

                sendingqueue = SendingQueues.OrderByDescending(x => x.Value.Priority).FirstOrDefault();
                if (sendingqueue.Value == null) return;

                var gsmModemHelper = new GsmModemHelper(modemPreference.ComPort) { OnActivityChanged = OnActivityChanged };
                gsmModemHelper.SendMessage(sendingqueue.Value.MobileNumber, sendingqueue.Value.TextMessage);
                RemoveForSendingMessage(sendingqueue.Value, false);
            }
            catch (Exception ex)
            {
                if (sendingqueue.Value != null) RemoveForSendingMessage(sendingqueue.Value, true);
                ErrorLogHelper<GatewayRoutine>.LogError(ex);
            }
        }

        private void GetMessageProcess(ModemPreference modemPreference)
        {
            try
            {
                if (!_canProcess || modemPreference == null || ActiveSetting == null || ActiveModemDefinition == null) return;
                var modemops = new ModemOperation() { OnActivityChanged = OnActivityChanged };
                var result = modemops.GetMessages(modemPreference);
                if (result == null || result.Count < 1) return;
                foreach (var smsMessage in result)
                {
                    SaveInboxMessage(smsMessage);
                    AddReceivedMessage(smsMessage);
                }
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
        }


        //REMOVE MESSAGE FROM QUEUE
        private void RemoveForSendingMessage(SmsMessage message, bool isfailed)
        {
            try
            {
                SmsMessage outboxMessage;
                SaveOutboxMessage(message, isfailed ? MessageStatus.Failed : MessageStatus.Sent);
                SendingQueues.TryRemove(message.MessageId, out outboxMessage);
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
            finally
            {
                if (isfailed) InvokeOnSentFailed();
                else InvokeOnSentCompleted();
            }
        }

        //ADD RECEIVE MESSAGE TO QUEUE
        private void AddReceivedMessage(SmsMessage message)
        {
            try
            {
                if (ReceivedQueues == null) ReceivedQueues = new ConcurrentDictionary<string, SmsMessage>();
                ReceivedQueues.TryAdd(message.MessageId, message);
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
            finally { InvokeOnReceivedCompleted(); }
        }

        #endregion SUB PROCESS

        #region DATABASE OPERATIONS

        private void SaveOutboxMessage(SmsMessage message, MessageStatus status)
        {
            try
            {
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
        }

        private void SaveInboxMessage(SmsMessage message)
        {
            return;
            ManagerInbox manager = null;
            try
            {
                manager = new ManagerInbox();
                manager.AddInbox(message.MobileNumber, message.TextMessage, message.ReceivedOn);
            }
            catch (Exception ex) { ErrorLogHelper<GatewayRoutine>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
        }


        #endregion DATABASE OPERATIONS

        #region PRIVATE PROPERTIES
        private ConcurrentQueue<ModemPreference> ActiveModemPreference { get; set; }
        private BackgroundWorker ProcessWorker { get; set; }

        private volatile bool _canProcess;

        private ProcessState ShadowCurrentProcessState { get; set; }

        private ProcessState LocalCurrentProcessState
        {
            get { return ShadowCurrentProcessState; }
            set
            {
                if (ShadowCurrentProcessState != value)
                {
                    //invoke process changed
                }
                ShadowCurrentProcessState = value;
            }
        }
        private DateTime LastSent { get; set; }
        private volatile SystemSetting ActiveSetting;
        private volatile ModemDefinition ActiveModemDefinition;
        #endregion PRIVATE PROPERTIES

        #region ACTION INVOKERS

        private void InvokeOnSignalChanged(SignalStrength strength)
        {
            if (OnSignalChanged == null) return;
            OnSignalChanged.BeginInvoke(strength, null, null);
        }

        private void InvokeOnSentCompleted()
        {
            if (OnSentCompleted == null) return;
            OnSentCompleted.BeginInvoke(null, null);
        }

        private void InvokeOnReceivedCompleted()
        {
            if (OnReceivedCompleted == null) return;
            OnReceivedCompleted.BeginInvoke(null, null);
        }

        private void InvokeOnSentFailed()
        {
            if (OnSentFailed == null) return;
            OnSentFailed.BeginInvoke(null, null);
        }

        #endregion ACTION INVOKERS

        #region SINGLETON DEFINITION

        private static GatewayRoutine _instance = null;
        private static readonly object Padlock = new object();

        public static GatewayRoutine Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Padlock)
                {
                    if (_instance != null) return _instance;
                    var newValue = new GatewayRoutine();
                    System.Threading.Thread.MemoryBarrier();
                    _instance = newValue;
                }
                return _instance;
            }
        }

        #endregion SINGLETON DEFINITION
    }
}