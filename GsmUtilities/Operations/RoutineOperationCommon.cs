using System;
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

        #endregion

        #region GET SIGNAL

        #endregion

        #region SEND MESSAGE

        #endregion

        #region RECEIVED MESSAGE

        #endregion

        #region DELETE MESSAGE

        #endregion

    }
}