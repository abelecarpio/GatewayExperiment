using GsmUtilities.Helpers.PduHelpers.Models;
using System;
using System.Collections;

namespace GsmUtilities.Helpers.PduHelpers
{
    //TODO: MAKE THIS A INTERNAL
    public class PduDecoder
    {
        public PduMessage Decode(string encodedMessage)
        {
            var returnValue = new PduMessage();
            var source = encodedMessage;
            if (string.IsNullOrEmpty(source)) throw new Exception("Invalid encoded pdu message.");

            returnValue.LocalServiceCenterNumber = PduHelper.PopServiceCenterAddress(ref source);
            returnValue.LocalPduType = PduHelper.PopPduType(ref source);
            var binbits = new BitArray(new byte[] { returnValue.PduType });
            if (binbits == null || binbits.Length < 8)
                throw new Exception("Invalid encoded pdu message, pdu type is not valid.");

            returnValue.LocalReplyPathExists = binbits[7];
            returnValue.LocalUserDataStartsWithHeader = binbits[6];
            returnValue.LocalStatusReportIndication = binbits[5];
            returnValue.LocalMoreMessages = binbits[3];


            returnValue.LocalValidityPeriodFormat = (ValidityPeriodFormat)(returnValue.PduType & 0x18);
            returnValue.LocalSmsDirection = (SmsDirection)(returnValue.PduType & 1);

            returnValue.LocalMessageReference = (returnValue.SmsDirection != SmsDirection.Submited)
                ? returnValue.MessageReference
                : PduHelper.PopMessageReference(ref source);

            returnValue.LocalPhoneNumber = PduHelper.PopPhoneNumber(ref source);
            returnValue.LocalProtocolIdentifier = PduHelper.PopProtocolIdentifier(ref source);
            returnValue.LocalDataCodingSheme = PduHelper.PopDataCodingSheme(ref source);
            returnValue.LocalValidityPeriod = (returnValue.SmsDirection != SmsDirection.Submited)
                ? returnValue.ValidityPeriod
                : PduHelper.PopValidityPeriod(ref source);

            returnValue.LocalServiceCenterTimeStamp = (returnValue.SmsDirection != SmsDirection.Received)
                ? returnValue.ServiceCenterTimeStamp
                : PduHelper.PopServiceCenterTimeStamp(ref source);

            returnValue.LocalUserData = source;
            if (string.IsNullOrEmpty(source)) return returnValue;

            var userDataLength = PduHelper.PopUserDataLength(ref source);
            returnValue.LocalMessageLength = userDataLength;

            if (userDataLength < 1) return returnValue;
            var userdataheaderlength = 0;
            if (returnValue.UserDataStartsWithHeader)
            {
                userdataheaderlength = PduHelper.PopUserDataHeaderLength(ref source);
                returnValue.LocalUserDataHeader = PduHelper.PopUserDataHeader(ref source, userdataheaderlength);
            }
            if (userDataLength < 1) return returnValue;
            returnValue.LocalMessage = PduHelper.DecodeMessage(returnValue.LocalDataCodingSheme, source, userDataLength, userdataheaderlength);
            //returnValue.LocalMessage = returnValue.LocalMessage.Substring(
            //    userdataheaderlength + (returnValue.UserDataStartsWithHeader ? 2 : 0
            //    ));
            return returnValue;
        }
    }
}