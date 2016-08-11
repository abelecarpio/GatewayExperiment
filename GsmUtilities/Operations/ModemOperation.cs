using GsmUtilities.Helpers;
using GsmUtilities.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;

namespace GsmUtilities.Operations
{
    public class ModemOperation
    {
        #region PUBLIC PROPERTIES

        internal Action<string> OnActivityChanged { get; set; }
        internal Action<object> OnModemListingOperationCompleted { get; set; }

        #endregion PUBLIC PROPERTIES

        #region GET AVAILABLE MODEM

        public void GetAvailableModem()
        {
            ProcessWorker = new BackgroundWorker();
            ProcessWorker.DoWork += GetAvailableModemOnDoWork;
            ProcessWorker.RunWorkerCompleted += GetAvailableModemOnRunWorkerCompleted;
            ProcessWorker.RunWorkerAsync();
        }

        private void GetAvailableModemOnDoWork(object sender, DoWorkEventArgs args)
        {
            var returnValue = new ConcurrentBag<ModemPreference>();
            try
            {
                var ports = SerialPort.GetPortNames();
                CommunicationLogHelper.LogInformation(string.Format("{0} COM Port detected.\r\n", ports.Length));
                foreach (var modemhelper in ports.Select(port => new GsmModemHelper(port) { OnActivityChanged = NotifyActivityChanged }).Where(modemhelper => modemhelper.IsPortIsModem))
                {
                    returnValue.Add(modemhelper.ComPortPreference);
                }
            }
            catch (Exception ex) { ErrorLogHelper<ModemOperation>.LogError(ex); }
            args.Result = returnValue;
        }

        private void GetAvailableModemOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            ProcessWorker.DoWork -= GetAvailableModemOnDoWork;
            ProcessWorker.RunWorkerCompleted -= GetAvailableModemOnRunWorkerCompleted;
            NotifyOperationCompleted(args.Result);
        }

        #endregion GET AVAILABLE MODEM

        #region GET MESSAGES

        internal List<SmsMessage> GetMessages(ModemPreference modem)
        {
            try
            {
                if (modem == null) return new List<SmsMessage>();
                var modemhelper = new GsmModemHelper(modem.ComPort) { OnActivityChanged = NotifyActivityChanged };
               return modemhelper.GetMessages();
            }
            catch (Exception ex) { ErrorLogHelper<ModemOperation>.LogError(ex); }
            return new List<SmsMessage>();
        }
       
        #endregion GET MESSAGES


        #region PRIVATE PROPERTIES

        private BackgroundWorker ProcessWorker { get; set; }

        private ComPortHelper ComHelper { get; set; }

        #endregion PRIVATE PROPERTIES

        #region NOTIFIER

        private void NotifyActivityChanged(string message = null)
        {
            if (OnActivityChanged == null) return;
            if (string.IsNullOrEmpty(message)) return;
            OnActivityChanged.BeginInvoke(message, null, null);
        }

        private void NotifyOperationCompleted(object result = null)
        {
            if (OnModemListingOperationCompleted == null) return;
            OnModemListingOperationCompleted.BeginInvoke(result, null, null);
        }

        #endregion NOTIFIER
    }
}