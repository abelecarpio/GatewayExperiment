using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;
using GsmUtilities.Helpers.PduHelpers;
using GsmUtilities.Models;

namespace GsmUtilities.Operations
{
    public partial class RoutineOperation
    {

        public void Start(SystemSetting setting, ModemDefinition modem)
        {
            if (setting == null) throw new ArgumentNullException("setting", @"System Setting should not be empty.");
            if (modem == null) throw new ArgumentNullException("modem", @"Modem Definition should not be empty.");
            SignalState = InternalSignalStrength.None;
            ActiveModem = modem;
            ActiveSystem = setting;

            ActivePort = new SerialPort(modem.ComPort)
            {
                Encoding = Encoding.UTF8,
                WriteTimeout = (modem.SendingTimeout > 500) ? modem.SendingTimeout : 500,
                ReadTimeout = (modem.SendingTimeout > 500) ? modem.SendingTimeout : 500,
                RtsEnable = true,
                DtrEnable = true,
                Handshake = Handshake.None,
                BaudRate = modem.BaudRate
            };
            ActivePort.Open();

            ProcessWorker = new BackgroundWorker();
            ProcessWorker.DoWork += ProcessWorkerOnDoWork;
            ProcessWorker.RunWorkerAsync();
            LocalIsRoutineRunning = true;
        }

        public void Stop()
        {
            if (ProcessWorker != null)
            {
                _abortProcess = true;
                while (ProcessWorker.IsBusy || ProcessWorker != null) { Thread.Sleep(1); }
            }
            if (ActivePort == null) return;
            if (ActivePort.IsOpen) ActivePort.Close();
            ActivePort = null;
            LocalIsRoutineRunning = false;
            SignalState = InternalSignalStrength.None;
        }

        public void SendMessage(SmsMessage message) { OutboxMessages.Enqueue(message); }

        #region PRIVATE FUNCTIONS

        private void ProcessWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            while (!_abortProcess)
            {
                try
                {
                    if (_abortProcess) continue;
                    else Thread.Sleep(1000);

                    GetSignalStrengthRoutine();

                    if (_abortProcess) continue;
                    else Thread.Sleep(1000);

                    SendMessageRoutine();

                    if (_abortProcess) continue;
                    else Thread.Sleep(1000);

                    ReceivedMessageRoutine();
                }
                catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
            }
            ProcessWorker.DoWork -= ProcessWorkerOnDoWork;
            ProcessWorker = null;
        }

        private void GetSignalStrengthRoutine()
        {
            PushInitialAtCommand();
            PushEnableErrorCommand();
            SignalState = PushAndGetSignalStrength();
            NotifyOnSignalStrengthChanged(SignalState);
        }

        private void SendMessageRoutine()
        {
            try
            {
                PushInitialAtCommand();
                PushEnableErrorCommand();
                var messageCenter = PushAndGetMessageCenter();
                if (string.IsNullOrEmpty(messageCenter)) return;
                SmsMessage queued;
                OutboxMessages.TryDequeue(out queued);
                if (queued == null) return;
                var encoder = new PduEncoder();
                var encodedmessage = encoder.Encode(queued.MobileNumber, queued.TextMessage, messageCenter);
                if (encodedmessage == null || encodedmessage.Count < 1) return;
                PushPduModeCommand();
                foreach (var codedmessage in encodedmessage)
                {
                    PushSendMessageCommand(codedmessage.Value, codedmessage.Key);  
                }
            }
            catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
        }

        private void ReceivedMessageRoutine()
        {
            try
            {
                PushInitialAtCommand();
                PushEnableErrorCommand();
                var rawdata = PushAndGetMessages();
                var smslist = ParseIncomingMessage(rawdata);
                foreach (var smsMessage in smslist)
                {
                    InboxMessages.Enqueue(smsMessage);
                    NotifyOnReceivedQueueChanged();
                }
            }
            catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
        }

        #endregion PRIVATE FUNCTIONS

        #region PROPERTIES
        private volatile bool _abortProcess;
        private SerialPort ActivePort { get; set; }
        private SystemSetting ActiveSystem { get; set; }
        private ModemDefinition ActiveModem { get; set; }
        private BackgroundWorker ProcessWorker { get; set; }
        private bool LocalIsRoutineRunning { get; set; }
        public bool IsRoutineRunning { get { return LocalIsRoutineRunning; } }
        internal InternalSignalStrength SignalState { get; set; }

        #endregion PROPERTIES

        #region FOR MESSAGES

        #region INBOX

        private ConcurrentQueue<SmsMessage> InboxMessages { get; set; }
        private BackgroundWorker ReceivedProcessor { get; set; }

        #endregion

        #region OUTBOX
        private ConcurrentQueue<SmsMessage> OutboxMessages { get; set; }

        #endregion

        #endregion


    }
}