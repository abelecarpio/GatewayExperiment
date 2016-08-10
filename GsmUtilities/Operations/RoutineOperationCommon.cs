using System;
using System.Linq;
using System.Threading;
using GsmUtilities.Helpers;

namespace GsmUtilities.Operations
{
    public partial class RoutineOperation
    {
        #region CALLBACK SETTINGS
        private void AddCallback()
        {
            ActivePort.DataReceived += ActivePortOnDataReceived;
            ActivePort.ErrorReceived += ActivePortOnErrorReceived;
        }
        private void RemoveCallback()
        {
            ActivePort.DataReceived -= ActivePortOnDataReceived;
            ActivePort.ErrorReceived -= ActivePortOnErrorReceived;
        }

        #endregion

        #region PARSE RECEIVED DATA
        private void ParseReceivedData(string receiveData)
        {
            if (string.IsNullOrEmpty(receiveData)) return;
            if (!receiveData.Contains(@"^RSSI")) return;
            var splited = receiveData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length < 1 || string.IsNullOrEmpty(splited[0])) return;
            splited = splited[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length < 2 || string.IsNullOrEmpty(splited[1])) return;
            if (!CommonHelpers.IsNumeric(splited[1])) return;

            var signalstrength = Convert.ToInt32(splited[1]);

            if (signalstrength < 2 || signalstrength > 98)
            {
                SignalState = InternalSignalStrength.Offline;
                NotifyOnSignalStrengthChanged(SignalStrength.Offline);
                return;
            }

            if (signalstrength > 1 && signalstrength < 10)
            {
                SignalState = InternalSignalStrength.Low;
                NotifyOnSignalStrengthChanged(SignalStrength.Low);
                return;
            }

            if (signalstrength > 9 && signalstrength < 15)
            {
                SignalState = InternalSignalStrength.Fair;
                NotifyOnSignalStrengthChanged(SignalStrength.Fair);
                return;
            }
            if (signalstrength > 14 && signalstrength < 20)
            {
                SignalState = InternalSignalStrength.Good;
                NotifyOnSignalStrengthChanged(SignalStrength.Good);
                return;
            }
            SignalState = InternalSignalStrength.Excellent;
            NotifyOnSignalStrengthChanged(SignalStrength.Excellent);
        }
        #endregion

        #region COMMON AT COMMANDS

        private void PushInitialAtCommand()
        {
            const string inputcommand = GsmCommand.PLAIN_AT;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
        }
        
        private void PushEnableErrorCommand()
        {
            const string inputcommand = GsmCommand.ENABLE_ERROR;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
        }

        private string PushAndGetMessages()
        {
            const string inputcommand = GsmCommand.LIST_AVAILABLE_MESSAGES;
            NotifyInputSubcriber(inputcommand);
           return ExecuteSync(inputcommand, GsmCommand.OK_RESPONSE);
        }

        private string CleanUpResult(string rawData)
        {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;
            if (!rawData.Contains(@"^RSSI")) return rawData;
            var returnValue = string.Empty;
            var splited = rawData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return (splited.Where(splitedData => !splitedData.Contains(@"^RSSI"))
                .Aggregate(returnValue, (current, splitedData) => string.Format("{0} {1}", current, splitedData.Trim()))).Trim(); 
        }

        #endregion

        #region GET SIGNAL

        #endregion

        #region SEND MESSAGE

        #endregion

        #region RECEIVED MESSAGE

        #endregion

        #region DELETE MESSAGE

        #endregion


        private void SetCommandEnd()
        {
            CommandEndTimeout = CommandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(CommandTimeout);
        }
        private string ExecuteSync(string command, string expectedResult, int timeout = 5000, bool raiseError = true)
        {
            var hasError = false;
            try
            {
                RemoveCallback();
                IssuedCommand = command;
                CommandTimeout = timeout;
                SetCommandEnd();
                ActivePort.WriteLine(command);
                Thread.Sleep(10);
                while (ActivePort.BytesToRead < 1)
                {
                    Thread.Sleep(1);
                    if (DateTime.Now < CommandEndTimeout) continue;
                    hasError = true;
                    if (raiseError) throw new Exception(string.Format("Command Timeout, no response from {0}", ActivePort.PortName));
                    break;
                }
                if (hasError) return CommandResult;

                CommandResult = ActivePort.ReadExisting();
                SetCommandEnd();
                while (!CommandResult.Contains(expectedResult) && !CommandResult.Contains("ERROR"))
                {
                    Thread.Sleep(1);
                    if (DateTime.Now < CommandEndTimeout) continue;
                    if (raiseError) throw new Exception(string.Format("Reading Timeout, no response from {0}", ActivePort.PortName));
                    break;
                }
            }
            catch (Exception ex)
            {
                if (raiseError) throw;
                ErrorLogHelper<RoutineOperation>.LogError(ex);
            }
            finally { AddCallback(); }
            return CleanUpResult(CommandResult);
        }

        private string CommandResult { get; set; }
        private string IssuedCommand { get; set; }
        private int CommandTimeout { get; set; }
        private DateTime CommandEndTimeout { get; set; }
    }
}