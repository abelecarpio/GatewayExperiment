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

        private void NotifyOnSignalStrengthChanged(SignalStrength signal)
        {
            if (OnSignalStrengthChanged == null) return;
            OnSignalStrengthChanged.BeginInvoke(signal, null, null);
        }



    }
}