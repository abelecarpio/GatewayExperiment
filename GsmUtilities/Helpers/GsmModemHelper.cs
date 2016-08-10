using GsmUtilities.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using GsmUtilities.Helpers.PduHelpers;

namespace GsmUtilities.Helpers
{
    public class GsmModemHelper
    {
        #region CONSTRUCTOR

        public GsmModemHelper(string comport)
        {
            if (string.IsNullOrEmpty(comport)) throw new ArgumentException("COM Port should not be empty.", "comport");
            SerialHelper = new SerialPortHelper(comport);
        }

        #endregion CONSTRUCTOR

        #region PUBLIC PROPERTIES

        public Action<string> OnActivityChanged { get; set; }

        public ModemPreference ComPortPreference { get { return ShadowModemPreference; } }

        #endregion PUBLIC CALLBACKS

        #region PUBLIC FUNCTIONS

        public bool IsPortIsModem
        {
            get
            {
                if (!DoesComPortIsModem()) return false;
                GetAdditionalModemDetail();
                return true;
            }
        }

        public SignalStrength GetModemSignalStrength()
        {
            try
            {
                PushPlainAtCommand();
                PushEnableErrorCommand();
                return GetAndPushSignalStrength();
            }
            catch (Exception ex) { ErrorLogHelper<GsmModemHelper>.LogError(ex); }
            finally { if (SerialHelper != null) SerialHelper.Dispose(); }
            return SignalStrength.Offline;
        }


        public List<SmsMessage> GetMessages()
        {
            var returnValue = new List<SmsMessage>();
            try
            {
                PushPlainAtCommand();
                PushEnableErrorCommand();
                var rawdata = PushAndGetMessages();
                return ParseIncomingMessage(rawdata);
            }
            catch (Exception ex) { ErrorLogHelper<GsmModemHelper>.LogError(ex); }
            finally { if (SerialHelper != null) SerialHelper.Dispose(); }
            return returnValue;
        }


        public void SendMessage(string mobileNumber, string message)
        {
            try
            {
                PushPlainAtCommand();
                PushEnableErrorCommand();
                var messageCenter = GetAndPushMessageCenter();
                var encoder = new PduEncoder();
                var encodedmessage = encoder.Encode(mobileNumber, message, messageCenter);
                if (encodedmessage == null || encodedmessage.Count < 1) return;
                PushPduModeCommand();
                for (var index = 0; index < encodedmessage.Count; index++)
                {
                    var pdumessage = encodedmessage.Dequeue();
                    PushTpduLength(pdumessage.Key);
                    PushPduData(pdumessage.Value);
                }
            }
            finally { if (SerialHelper != null) SerialHelper.Dispose(); }
        }




        #endregion PUBLIC FUNCTIONS

        #region PRIVATE FUNCTIONS

        private bool DoesComPortIsModem()
        {
            try
            {
                ShadowModemPreference = new ModemPreference()
                {
                    ComPort = SerialHelper.SerialIoPort.PortName,
                    BaudRate = SerialHelper.SerialIoPort.BaudRate
                };
                PushPlainAtCommand();
                PushEnableErrorCommand();
                ShadowModemPreference.Imei = GetAndPushImeiCommand();
                return true;
            }
            catch (Exception ex)
            {
                if (SerialHelper != null) SerialHelper.Dispose();
                ErrorLogHelper<GsmModemHelper>.LogError(ex);
            }
            return false;
        }

        private void GetAdditionalModemDetail()
        {
            try
            {
                ShadowModemPreference.Manufacturer = GetAndPushManufacturer();
                ShadowModemPreference.ModemModel = GetAndPushModel();
            }
            catch (Exception ex) { ErrorLogHelper<GsmModemHelper>.LogError(ex); }
            finally { if (SerialHelper != null) SerialHelper.Dispose(); }
        }

        #endregion PRIVATE FUNCTIONS

        #region COMMON FUNCTIONS

        private void PushPlainAtCommand()
        {
            const string inputcommand = GsmCommand.PLAIN_AT;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
        }

        private void PushEnableErrorCommand()
        {
            const string inputcommand = GsmCommand.ENABLE_ERROR;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
        }

        private void PushPduModeCommand()
        {
            const string inputcommand = GsmCommand.SET_PDU_MODE;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
        }

        private string GetAndPushImeiCommand()
        {
            const string inputcommand = GsmCommand.GET_IMEI_NUMBER;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
            return string.IsNullOrEmpty(result) ? string.Empty : result.Replace(GsmCommand.OK_RESPONSE, "").Trim();
        }

        private string GetAndPushManufacturer()
        {
            const string inputcommand = GsmCommand.GET_MANUFACTURER;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
            return string.IsNullOrEmpty(result) ? string.Empty : result.Replace(GsmCommand.OK_RESPONSE, "").Trim();
        }

        private string GetAndPushModel()
        {
            const string inputcommand = GsmCommand.GET_MODEL_NUMBER;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);
            return string.IsNullOrEmpty(result) ? string.Empty : result.Replace(GsmCommand.OK_RESPONSE, "").Trim();
        }

        private string GetAndPushMessageCenter()
        {
            const string inputcommand = GsmCommand.GET_MESSAGE_CENTER;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.MESSAGE_CENTER_RESPONSE);
            NotifyOutputSubcriber(result);

            if (string.IsNullOrEmpty(result)) throw new Exception("Unable to get Message Center, result is null.");
            var splitedResult = result.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 2) throw new Exception("Unable to get Message Center.");
            splitedResult = splitedResult[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 2)
                throw new Exception("Unable to get Message Center. Unable to extract the message center from response code.");
            splitedResult = splitedResult[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (splitedResult.Length < 1 || string.IsNullOrEmpty(splitedResult[0]))
                throw new Exception("Unable to get Message Center. Unable to extract the message center.");

            return splitedResult[0].Replace("\"", "").Trim();
        }

        private void PushTpduLength(int tpduLength)
        {
            var inputcommand = GsmCommand.SEND_MESSAGE + tpduLength + "\r";
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.START_MESSAGE_RESPONSE);
            NotifyOutputSubcriber(result);
        }

        private void PushPduData(string pdudata)
        {
            var inputcommand = pdudata;
                //+ GsmCommand.CONTROL_Z;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.Execute(inputcommand, GsmCommand.OK_RESPONSE, -1, false);
            NotifyOutputSubcriber(result);
        }

        private SignalStrength GetAndPushSignalStrength()
        {
            const string inputcommand = GsmCommand.GET_SIGNAL_STRENGTH;
            NotifyInputSubcriber(inputcommand);
            var result = SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
            NotifyOutputSubcriber(result);

            if (string.IsNullOrEmpty(result)) return SignalStrength.Offline;
            if (result.ToUpper().Contains("ERROR")) throw new Exception(result);
            result = result.Replace(GsmCommand.OK_RESPONSE, "").Trim();
            var signal = result.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (signal.Length < 2) return SignalStrength.Offline;
            signal = signal[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (signal.Length < 2) return SignalStrength.Offline;

            var signalstrength = Convert.ToInt32(signal[1]);

            if (signalstrength < 2 || signalstrength > 98)
                return SignalStrength.Offline;
            if (signalstrength > 1 && signalstrength < 10)
                return SignalStrength.Low;
            if (signalstrength > 9 && signalstrength < 15)
                return SignalStrength.Fair;
            if (signalstrength > 14 && signalstrength < 20)
                return SignalStrength.Good;
            return SignalStrength.Excellent;
        }

        private string PushAndGetMessages()
        {
            const string inputcommand = GsmCommand.LIST_AVAILABLE_MESSAGES;
            NotifyInputSubcriber(inputcommand);
            return SerialHelper.ExecuteCommand(inputcommand, GsmCommand.OK_RESPONSE);
        }


        private void PushDeleteCommand(int messageId)
        {
            
        }


        private List<SmsMessage> ParseIncomingMessage(string rawdata)
        {
            if (string.IsNullOrEmpty(rawdata)) return new List<SmsMessage>();
            var returnValue = new List<SmsMessage>();
            var splitedresult = rawdata.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var linecounter = 0;
            foreach (var result in splitedresult)
            {
                try
                {
                    linecounter++;
                    if (linecounter%2 > 0)
                    {
                        DeleteMessage(result);
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

        private void DeleteMessage(string referenceNumber)
        {
            if (string.IsNullOrEmpty(referenceNumber) || !referenceNumber.StartsWith("+CMGL")) return;
            var splited = referenceNumber.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length < 2) return;
            splited = splited[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (splited.Length <1) return;
            int messageid;
            if (!int.TryParse(splited[0], out messageid)) return;
            PushDeleteCommand(messageid);
        }

        #endregion COMMON FUNCTIONS

        #region NOTIFICATIONS

        private void NotifyInputSubcriber(string message)
        {
            if (string.IsNullOrEmpty(message) || OnActivityChanged == null) return;
            CommunicationLogHelper.LogInformation(message.AddPrefixWriteTimestamp());
            OnActivityChanged.BeginInvoke(message, null, null);
        }

        private void NotifyOutputSubcriber(string message)
        {
            if (string.IsNullOrEmpty(message) || OnActivityChanged == null) return;
            CommunicationLogHelper.LogInformation(message.AddPrefixReadTimestamp());
            OnActivityChanged.BeginInvoke(message, null, null);
        }

        #endregion NOTIFICATIONS

        #region PRIVATE PROPERTIES

        private SerialPortHelper SerialHelper { get; set; }
        private ModemPreference ShadowModemPreference { get; set; }

        #endregion PRIVATE PROPERTIES
    }
}