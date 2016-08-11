using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading;
using GsmUtilities.Helpers;
using GsmUtilities.Helpers.PduHelpers;
using GsmUtilities.Models;

namespace GsmUtilities.Operations
{
    public partial class RoutineOperation
    {

        #region COMMON AT COMMANDS

        private void PushInitialAtCommand()
        {
            const string inputcommand = GsmCommand.PLAIN_AT;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);
        }

        private void PushEnableErrorCommand()
        {
            const string inputcommand = GsmCommand.ENABLE_ERROR;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);
        }




        private string CleanUpResult(string rawData)
        {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;
            //if (!rawData.Contains(@"^RSSI")) return rawData;
            //var returnValue = string.Empty;
            //var splited = rawData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //return (splited.Where(splitedData => !splitedData.Contains(@"^RSSI"))
            //    .Aggregate(returnValue, (current, splitedData) => string.Format("{0} {1}", current, splitedData.Trim()))).Trim();

            return Regex.Replace(rawData, "(\\n|\\r|\\r\\n)\\^.*(\\n|\\r|\\r\\n)", "");
        }

        #endregion

        #region GET SIGNAL
        private InternalSignalStrength PushAndGetSignalStrength()
        {
            const string inputcommand = GsmCommand.GET_SIGNAL_STRENGTH;
            NotifyInputSubcriber(inputcommand);

            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);

            if (string.IsNullOrEmpty(result)) return InternalSignalStrength.Offline;

            if (result.ToUpper().Contains("ERROR")) throw new Exception(result);
            result = result.Replace(GsmCommand.OK_RESPONSE, "").Trim();
            var signal = result.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (signal.Length < 2) return InternalSignalStrength.Offline;
            signal = signal[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (signal.Length < 2) return InternalSignalStrength.Offline;

            var signalstrength = Convert.ToInt32(signal[1]);

            if (signalstrength < 2 || signalstrength > 98)
                return InternalSignalStrength.Offline;
            if (signalstrength > 1 && signalstrength < 10)
                return InternalSignalStrength.Low;
            if (signalstrength > 9 && signalstrength < 15)
                return InternalSignalStrength.Fair;
            if (signalstrength > 14 && signalstrength < 20)
                return InternalSignalStrength.Good;
            return InternalSignalStrength.Excellent;
        }

        #endregion

        #region SEND MESSAGE
        private string PushAndGetMessageCenter()
        {
            const string inputcommand = GsmCommand.GET_MESSAGE_CENTER;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);

            if (string.IsNullOrEmpty(result)) throw new Exception("Unable to get Message Center, result is null.");
            var splitedResult = result.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 2) throw new Exception("Unable to get Message Center.");
            splitedResult = splitedResult[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 2)
                throw new Exception("Unable to get Message Center. Unable to extract the message center from response code.");
            splitedResult = splitedResult[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 1 || string.IsNullOrEmpty(splitedResult[0]))
                throw new Exception("Unable to get Message Center. Unable to extract the message center.");

            return splitedResult[0].Replace("\"", "").Trim();
        }

        private void PushPduModeCommand()
        {
            const string inputcommand = GsmCommand.SET_PDU_MODE;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);
        }

        private void PushSendMessageCommand(string pdudata, int tpduLength)
        {
            var inputcommand = GsmCommand.SEND_MESSAGE + tpduLength + "\r" + pdudata + GsmCommand.CONTROL_Z;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand, ActiveModem.SendingTimeout);
            NotifyOutputSubcriber(result);
        }

        #endregion

        #region RECEIVED MESSAGE

        private string PushAndGetMessages()
        {
            const string inputcommand = GsmCommand.LIST_AVAILABLE_MESSAGES;
            NotifyInputSubcriber(inputcommand);
            return ExecuteSync(inputcommand);
        }

        #endregion

        #region DELETE MESSAGE

        private void PushDeleteMessageCommand(string rawId)
        {
            if (string.IsNullOrEmpty(rawId)) return;
            var splited = rawId.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length < 2 && !splited[0].Contains("+CMGL") ) return;
            splited = splited[1].Trim().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length < 1 && !CommonHelpers.IsNumeric(splited[0])) return;
            var inputcommand = GsmCommand.DELETE_MESSAGE + splited[0].Trim() + GsmCommand.END_OF_LINE;
            NotifyInputSubcriber(inputcommand);
            var result = ExecuteSync(inputcommand);
            NotifyOutputSubcriber(result);
        }

        #endregion


        private void SetCommandEnd()
        {
            CommandEndTimeout = CommandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(CommandTimeout);
        }
        private string ExecuteSync(string command, int timeout = 5000, bool raiseError = true)
        {
            var hasError = false;
            try
            {
                CommandResult = string.Empty;
                IssuedCommand = command;
                CommandTimeout = timeout;
                SetCommandEnd();
                ActivePort.ReadTimeout = timeout;
                ActivePort.WriteTimeout = timeout;
                ActivePort.WriteLine(command);
                Thread.Sleep(10);
                while (ActivePort.BytesToRead < 1)
                {
                    Thread.Sleep(1);
                    if (DateTime.Now < CommandEndTimeout) continue;
                    hasError = true;
                    if (raiseError)
                        throw new Exception(string.Format("Command Timeout, no response from {0}", ActivePort.PortName));
                    break;
                }

                if (hasError) return CommandResult;

                SetCommandEnd();
                while (ActivePort.BytesToRead > 0)
                {
                    SetCommandEnd();
                    Thread.Sleep(1);
                    CommandResult += ActivePort.ReadLine();
                    if (DateTime.Now < CommandEndTimeout) continue;
                    if (raiseError)
                        throw new Exception(string.Format("Reading Timeout, no response from {0}", ActivePort.PortName));
                    break;
                }
                if (CommandResult.Contains("ERROR") && raiseError)
                    throw new Exception(CommandResult);
            }
            catch (Exception ex)
            {
                if (raiseError) throw;
                ErrorLogHelper<RoutineOperation>.LogError(ex);
            }
            finally
            {
                ActivePort.DiscardInBuffer();
                ActivePort.DiscardOutBuffer();
            }
            return CleanUpResult(CommandResult);
        }

        private string CommandResult { get; set; }
        private string IssuedCommand { get; set; }
        private int CommandTimeout { get; set; }
        private DateTime CommandEndTimeout { get; set; }



        #region RECEIVED FUNCTIONS

        private void EnqueueReceived(string rawData)
        {
            try
            {

            }
            catch (Exception ex) { ErrorLogHelper<RoutineOperation>.LogError(ex); }
        }

        private void DequeueReceived(string messageId)
        {
            
        }

        #endregion

        private List<SmsMessage> ParseIncomingMessage(string rawdata)
        {
            if (string.IsNullOrEmpty(rawdata)) return new List<SmsMessage>();
            var returnValue = new List<SmsMessage>();
            var splitedresult = rawdata.Trim().Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var linecounter = 0;
            foreach (var splited in splitedresult)
            {
                try
                {
                    linecounter++;
                    if (string.IsNullOrEmpty(splited)) continue;
                    var result = splited.Trim();
                    if (linecounter % 2 > 0)
                    {
                        //PushDeleteMessageCommand(result);
                        continue;
                    }
                    var decoder = new PduDecoder();
                    var decoded = decoder.Decode(result);
                    if (decoded == null) continue;
                    returnValue.Add(new SmsMessage()
                    {
                        MobileNumber = decoded.PhoneNumber,
                        TextMessage = decoded.Message,
                        LocalReceivedOn = decoded.ServiceCenterTimeStamp
                    });
                }
                catch (Exception ex) { ErrorLogHelper<GsmModemHelper>.LogError(ex); }
            }
            return returnValue;
        }




        public RoutineOperation()
        {
            InboxMessages = new ConcurrentQueue<SmsMessage>();
            OutboxMessages = new ConcurrentQueue<SmsMessage>();
        }
    }
}