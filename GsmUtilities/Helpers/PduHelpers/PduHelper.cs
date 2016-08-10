using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GsmUtilities.Helpers.PduHelpers
{
    public static partial class PduHelper
    {

        private static string PopAddress(ref string source, int length)
        {
            if (string.IsNullOrEmpty(source) || source.Length < length)
                throw new Exception("Invalid Pdu Source, incorrect SMSC Information");
            var address = source.Substring(0, length);
            PopData(ref source, length);
            var addressType = PopByte(ref address);
            address = string.Concat(SplitString(address, 2).Select(SwapCharacters)).Trim('F');
            var paddedAddressType = (addressType >> 4);

            switch (paddedAddressType)
            {
                case 9:
                    return string.Format("+{0}", address);
                case 10:
                    return address;
            }
            address = string.Concat(SplitString(address, 2).Select(SwapCharacters)).Trim('F');
            return DecodeSevenBit(address, address.Length / 2, 0);
        }

        internal static string PopServiceCenterAddress(ref string source)
        {
            int smscInfoLength = PopByte(ref source);
            return (smscInfoLength = (smscInfoLength) * 2) == 0
                ? string.Empty
                : PopAddress(ref source, smscInfoLength);
        }

        internal static byte PopPduType(ref string source)
        {
            return PopByte(ref source);
        }

        internal static byte PopMessageReference(ref string source)
        {
            return PopByte(ref source);
        }

        internal static string PopPhoneNumber(ref string source)
        {
            int numberLength = PopByte(ref source);
            return ((numberLength = numberLength + 2) == 2)
                ? string.Empty
                : PopAddress(ref source, numberLength + (numberLength % 2));
        }

        internal static byte PopProtocolIdentifier(ref string source)
        {
            return PopByte(ref source);
        }

        internal static byte PopDataCodingSheme(ref string source)
        {
            return PopByte(ref source);
        }

        internal static byte PopValidityPeriod(ref string source)
        {
            return PopByte(ref source);
        }

        internal static DateTime PopServiceCenterTimeStamp(ref string source)
        {
            return PopDateTime(ref source);
        }

        internal static int PopUserDataLength(ref string source)
        {
            return PopByte(ref source);
        }

        internal static byte PopUserDataHeaderLength(ref string source)
        {
            return Convert.ToByte(source.Substring(0, 2), 16);
        }

        internal static byte[] PopUserDataHeader(ref string source, int length)
        {
            length = length + ((length % 2) == 0 ? 0 : 1);
            return PopBytes(ref source, length);
        }

        internal static SmsType GetSmsType(string source)
        {
            var scalength = PeekByte(source, 0);
            var pduType = PeekByte(source, scalength + 1);
            var smsType = (byte)((pduType & 3) >> 1);
            if (!Enum.IsDefined(typeof(SmsType), (int)smsType))
                throw new UnknownSmsTypeException(pduType);

            return (SmsType)smsType;
        }

        private static byte PeekByte(string source, int byteIndex)
        {
            return Convert.ToByte(source.Substring(byteIndex * 2, 2), 16);
        }

        private static string GetEscapeCharacter(int character)
        {
            if (character < 1) return string.Empty;
            switch (character)
            {
                case 101:
                    return @"€";

                case 10:
                    return Environment.NewLine;

                case 60:
                    return @"[";

                case 47:
                    return @"\";

                case 62:
                    return @"]";

                case 20:
                    return @"^";

                case 40:
                    return @"{";

                case 64:
                    return @"|";

                case 41:
                    return @"}";

                case 61:
                    return @"~";

                default:
                    return string.Empty;
            }
        }

        internal static string DecodeMessage(byte dataCodingSheme, string userdata, int userdatalength, int userdataheaderlength)
        {
            //userdatalength = userdata.Length / 2;
            switch ((SmsEncoding)dataCodingSheme & SmsEncoding.ReservedMask)
            {
                case SmsEncoding.SevenBit:
                    return DecodeSevenBit(userdata, userdatalength, userdataheaderlength);

                case SmsEncoding.EightBit:
                    return DecodeEightBit(userdata, userdatalength);

                case SmsEncoding.Ucs2:
                    return DecodeUcs2(userdata, userdatalength);

                case SmsEncoding.ReservedMask:
                default:
                    return string.Empty;
            }
        }

        private static string DecodeSevenBit(string source, int length, int userdataheaderlength)
        {
            if (string.IsNullOrEmpty(source) || length < 1) return string.Empty;
            var bytes = GetBytes(source, 16);
            Array.Reverse(bytes);

            var binary = bytes
                .Aggregate(string.Empty,
                (current, b) => current + Convert.ToString(b, 2).PadLeft(8, '0'));

            binary = binary.PadRight(length * 7, '0');

            var tmpresult = string.Empty;

            for (var i = 1; i <= length; i++)
            {
                var bytepart = binary.Substring(binary.Length - i * 7, 7);
                tmpresult += (char)Convert.ToByte(bytepart, 2);
            }
            if (string.IsNullOrEmpty(tmpresult)) return tmpresult;
            if (!tmpresult.ToCharArray().Any(x => x == 27 || x == 0)) return tmpresult;

            var finalresult = string.Empty;
            var resultArray = tmpresult.ToCharArray();
            userdataheaderlength = userdataheaderlength + ((userdataheaderlength % 2) != 0 ? 1 : 0);
            for (var index = userdataheaderlength < 1 ? 0 : userdataheaderlength + 1;
                index < resultArray.Length; 
                index++)
            {
                if (resultArray[index] == 0)
                {
                    finalresult += @"@";
                    continue;
                }

                if (resultArray[index] != 27)
                {
                    finalresult += resultArray[index];
                    continue;
                }
                finalresult += GetEscapeCharacter(Convert.ToInt32(resultArray[index + 1]));
                index++;
            }
            return finalresult;
        }

        private static string DecodeEightBit(string source, int length)
        {
            if (string.IsNullOrEmpty(source) || length < 1) return string.Empty;
            //var bytes = GetBytes(source.Substring(0, length * 2), 16);
            var bytes = GetBytes(source, 16);
            return Encoding.UTF8.GetString(bytes);
        }

        private static string DecodeUcs2(string source, int length)
        {
            if (string.IsNullOrEmpty(source) || length < 1) return string.Empty;
            //var bytes = GetBytes(source.Substring(0, length * 2), 16);
            var bytes = GetBytes(source, 16);
            return Encoding.BigEndianUnicode.GetString(bytes);
        }

        internal static void PushMobileNumberLength(ref string destination, string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile Number should not be empty", "mobileNumber");

            //4. Mobile Number Length without "+"
            destination += mobileNumber.Replace("+", "").Length.ToString("X2");
        }

        internal static void PushTypeOfAddress(ref string destination, string mobileNumber)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination should not be empty", "destination");

            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile Number should not be empty", "mobileNumber");

            //5. Type of Address, 91 for international format.
            destination += mobileNumber.StartsWith("+") ? "91" : "00";
        }

        internal static void PushMobileNumber(ref string destination, string mobileNumber)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination should not be empty", "destination");

            if (string.IsNullOrEmpty(mobileNumber))
                throw new ArgumentException("Mobile Number should not be empty", "mobileNumber");

            //6. Mobile Number
            destination += EncodeMobileNumber(mobileNumber);
        }

        internal static Queue<KeyValuePair<int, string>> ParseOutbox(string mobileNumber, string message,
            string messagecenter)
        {
            var returnValue = new Queue<KeyValuePair<int, string>>();

            #region SMSC Info
            var numberinfo = messagecenter.StartsWith("+") ? "91" : "00";
            var centernumber = EncodeMobileNumber(messagecenter);
            var smsclength = SplitString(string.Format("{0}{1}", numberinfo, centernumber), 2).Count().ToString("X2");
            var smscinfo = string.Format("{0}{1}{2}", smsclength, numberinfo, centernumber);
            #endregion

            #region Destination Info
            var destinationInfo = string.Empty;
            PushMobileNumberLength(ref destinationInfo, mobileNumber);
            PushTypeOfAddress(ref destinationInfo, mobileNumber);
            PushMobileNumber(ref destinationInfo, mobileNumber);
            #endregion

            var encoding = GetEncodingType(message);
            var splitedMessage = DivideMessage(message, encoding);
            var rand = new Random();
            var reference1 = rand.Next(0, 255);
            var reference2 = rand.Next(0, 255);
            var messagecounter = 1;

            foreach (var splitmessage in splitedMessage)
            {
                var udh = GenerateUserDataHeader(reference1, reference2, messagecounter, splitedMessage.Count);
                var result = EncodePdu(smscinfo, splitedMessage.Count > 1 ? "51" : "11", destinationInfo, udh, EncodeMessage(splitmessage, encoding), encoding);
                var datalength = (result.Length - smscinfo.Length) / 2;
                returnValue.Enqueue(new KeyValuePair<int, string>(datalength, result));
                messagecounter++;
            }

            return returnValue;
        }

        private static string EncodePdu(string smscinfo, string firstoctect, string destination, string userDataHeader, string userdata, SmsEncoding encoding)
        {
            var returnValue = string.Empty;

            returnValue = string.Format("{0}{1}", returnValue, smscinfo);                       // SMSC Info
            returnValue = string.Format("{0}{1}", returnValue, firstoctect);                    //  First Octect (SMS-SUBMIT)
            returnValue = string.Format("{0}00", returnValue);                                  //  Message Reference
            returnValue = string.Format("{0}{1}", returnValue, destination);                    //  Receiver Info
            returnValue = string.Format("{0}00", returnValue);                                  //  Protocol Identifier
            returnValue = string.Format("{0}{1}", returnValue,
                Convert.ToString((int)encoding, 16).PadLeft(2, '0'));                           //  Data Coding Scheme
            returnValue = string.Format("{0}AA", returnValue);                                  //  Validity Period = 4 days
            returnValue = string.Format("{0}{1}", returnValue, userdata.Substring(0, 2));        //  Message length
            userdata = userdata.Substring(2);                                                   //  Remove Message Length in userdata
            if (!string.IsNullOrEmpty(userdata))
                returnValue = string.Format("{0}{1}", returnValue, userDataHeader);             //  User data header
            returnValue = string.Format("{0}{1}", returnValue, userdata);                       //  Message including length

            return returnValue.ToUpper();
        }

    }


    internal class UnknownSmsTypeException : Exception
    {
        public UnknownSmsTypeException(byte pduType)
            : base(string.Format("Unknow SMS type. PDU type binary: {0}.", Convert.ToString(pduType, 2)))
        {
        }
    }
}