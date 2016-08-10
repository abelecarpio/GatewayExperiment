using System;
using System.Collections.Generic;

namespace GsmUtilities.Helpers.PduHelpers
{
    //TODO: MAKE THIS A INTERNAL
    public class PduEncoder
    {
        public Queue<KeyValuePair<int, string>> Encode(string mobileNumber, string message, string messageCenter)
        {
            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile Number should not be empty.", "mobileNumber");

            if (string.IsNullOrEmpty(messageCenter))
                throw new ArgumentException("Message Center should not be empty.", "messageCenter");

            message = (string.IsNullOrEmpty(message)) ? string.Empty : message;
            return PduHelper.ParseOutbox(mobileNumber, message, messageCenter);
        }


    }
}