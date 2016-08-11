using System;
using GsmUtilities.Helpers;

namespace GsmUtilities.Operations
{
    public partial class RoutineOperation
    {
        
        public Action<string> OnActivityChanged { get; set; }

        private void NotifyInputSubcriber(string message)
        {
            CommunicationLogHelper.LogInformation(message.AddPrefixWriteTimestamp());
            if (OnActivityChanged == null) return;
            OnActivityChanged.BeginInvoke(message, null, null);
        }

        private void NotifyOutputSubcriber(string message)
        {
            CommunicationLogHelper.LogInformation(message.AddPrefixReadTimestamp());
            if (OnActivityChanged == null) return;
            OnActivityChanged.BeginInvoke(message, null, null);
        }



        public Action OnReceivedQueueChanged { get; set; }
        private void NotifyOnReceivedQueueChanged()
        {
            if (OnReceivedQueueChanged == null) return;
            OnReceivedQueueChanged.BeginInvoke(null, null);
        }


        public Action OnSendQueueChanged { get; set; }

        private void NotifyOnSendQueueChanged()
        {
            if (OnSendQueueChanged == null) return;
            OnSendQueueChanged.BeginInvoke(null, null);
        }


        public Action<SignalStrength> OnSignalStrengthChanged { get; set; }

        private void NotifyOnSignalStrengthChanged(InternalSignalStrength signal)
        {
            if (OnSignalStrengthChanged == null) return;

            switch (signal)
            {
                case InternalSignalStrength.None:
                    OnSignalStrengthChanged.BeginInvoke(SignalStrength.Offline, null, null);
                    break;

                case InternalSignalStrength.Offline:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Offline, null, null);
                    break;

                case InternalSignalStrength.Low:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Low, null, null);
                    break;

                case InternalSignalStrength.Fair:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Fair, null, null);
                    break;

                case InternalSignalStrength.Good:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Good, null, null);
                    break;

                case InternalSignalStrength.Excellent:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Excellent, null, null);
                    break;

                default:
                      OnSignalStrengthChanged.BeginInvoke(SignalStrength.Offline, null, null);
                    break;
            }
        }



    }
}