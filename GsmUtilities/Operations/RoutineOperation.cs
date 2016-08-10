using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

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
                WriteTimeout = modem.SendingTimeout,
                ReadTimeout = modem.SendingTimeout,
                RtsEnable = true,
                DtrEnable = true
            };
            AddCallback();
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

        private void ProcessWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            while (!_abortProcess)
            {
                try
                {
                    if (_abortProcess) continue;
                    else Thread.Sleep(1);

                    GetSignalStrengthRoutine();

                    if (_abortProcess) continue;
                    else Thread.Sleep(1);

                    SendMessageRoutine();

                    if (_abortProcess) continue;
                    else Thread.Sleep(1);

                    ReceivedMessageRoutine();
                }
                catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
            }
            ProcessWorker.DoWork -= ProcessWorkerOnDoWork;
            ProcessWorker = null;
        }

        private void ActivePortOnErrorReceived(object sender, SerialErrorReceivedEventArgs args)
        {
            if (args == null) return;
            NotifyOutputSubcriber(args.ToString());
        }

        private void ActivePortOnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            if (sender == null) return;
            var privateserial = (SerialPort)sender;
            if (!privateserial.IsOpen) return;
            var dataReceived = privateserial.ReadExisting();
            NotifyOutputSubcriber(dataReceived);
            ParseReceivedData(dataReceived);
        }

        #region PRIVATE FUNCTIONS

        private void GetSignalStrengthRoutine()
        {
            try
            {
                if (SignalState != InternalSignalStrength.None) return;
                Thread.Sleep(1000);
            }
            catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
        }

        private void SendMessageRoutine()
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
        }

        private void ReceivedMessageRoutine()
        {
            try
            {
                Thread.Sleep(1000);
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

    }
}