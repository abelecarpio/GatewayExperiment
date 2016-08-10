using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsmUtilities.Helpers.PduHelpers
{
    public static partial class PduHelper
    {
        private static byte[] GetBytes(string source, int fromBase)
        {
            var returnValue = new List<byte>();

            for (var i = 0; i < source.Length / 2; i++)
                returnValue.Add(Convert.ToByte(source.Substring(i * 2, 2), fromBase));
            return returnValue.ToArray();
        }

        private static void PopData(ref string source, int length)
        {
            source = source.Substring(length);
        }

        private static byte PopByte(ref string source)
        {
            if (string.IsNullOrEmpty(source) || source.Length < 2)
                throw new Exception("Invalid Pdu Source.");
            var returnValue = Convert.ToByte(source.Substring(0, 2), 16);
            PopData(ref source, 2);
            return returnValue;
        }

        private static byte[] PopBytes(ref string source, int length)
        {
            var bytes = source.Substring(0, length * 2);
            return GetBytes(bytes, 16);
        }

        private static DateTime PopDateTime(ref string source)
        {
            var timestamp = source.Substring(0, 14);
            var bytes = GetBytes(string.Concat(SplitString(timestamp, 2).Select(SwapCharacters)), 10);
            PopData(ref source, 14);
            return new DateTime(2000 + bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]).AddMilliseconds(bytes[6]);
        }

        internal static List<string> SplitString(string unsplited, int chunkSize)
        {
            if (string.IsNullOrEmpty(unsplited) || chunkSize < 1) return new List<string>();
            return Enumerable.Range(0, unsplited.Length / chunkSize)
                .Select(i => unsplited.Substring(i * chunkSize, chunkSize)).ToList();
        }

        internal static string SwapCharacters(string rawdata)
        {
            if (string.IsNullOrEmpty(rawdata) || rawdata.Length < 2) return string.Empty;
            return string.Format("{0}{1}", rawdata[1], rawdata[0]);
        }

        private static string EncodeMobileNumber(string mobileNumber)
        {
            mobileNumber = mobileNumber.Replace("+", "");
            mobileNumber = string.Format("{0}{1}", mobileNumber, (mobileNumber.Length % 2 == 1) ? "F" : "");
            var chucked = SplitString(mobileNumber, 2);
            return chucked.Aggregate(string.Empty, (current, splited) => string.Format("{0}{1}", current, SwapCharacters(splited)));
        }

        private static SmsEncoding GetEncodingType(string message)
        {
            return message.ToCharArray().Select(Convert.ToInt32).ToList().Any(x => x != 8364 && x > 127) ? SmsEncoding.Ucs2 : SmsEncoding.SevenBit;
        }

        internal static List<string> DivideMessage(string message, SmsEncoding encoding)
        {
            if (string.IsNullOrEmpty(message)) return new List<string>() { "" };
            var additionalchar = new Dictionary<char, int>() { { '€', 101 }, { '\n', 10 }, { '[', 60 }, { '\\', 47 }, { ']', 62 }, { '^', 20 }, { '{', 40 }, { '|', 64 }, { '}', 41 }, { '~', 61 } };

            var returnValue = new List<string>();
            var characterCounter = 0;
            var messagepart = string.Empty;

            switch (encoding)
            {
                case SmsEncoding.SevenBit:
                    var tmpMessageArray = message.ToCharArray();
                    var additionalcount = tmpMessageArray.Where(x => additionalchar.Keys.Any(z => z == x) || x == 10).Count();
                    var maxdatacount = (message.Length + additionalcount) > 160 ? 150 : 160;

                    foreach (var charx in tmpMessageArray)
                    {
                        messagepart += charx;
                        characterCounter += (additionalchar.Keys.Any(x => x == charx) || Convert.ToInt32(charx) == 10) ? 2 : 1;
                        if (characterCounter < maxdatacount) continue;
                        returnValue.Add(messagepart);
                        messagepart = string.Empty;
                        characterCounter = 0;
                    }
                    if (!string.IsNullOrEmpty(messagepart)) returnValue.Add(messagepart);
                    break;

                case SmsEncoding.Ucs2:
                    returnValue = SplitString(message, 70);
                    if (returnValue.Count < 1) returnValue.Add(message);
                    break;
            }
            return returnValue;
        }

        private static string EncodeMessage(string message, SmsEncoding encoding)
        {
            var datalength = 0;
            var returnValue = string.Empty;
            if (string.IsNullOrEmpty(message)) return "00";
            switch (encoding)
            {
                case SmsEncoding.SevenBit:
                    returnValue = ConvertToSevenBitPack(message, ref datalength);
                    break;

                case SmsEncoding.Ucs2:
                    returnValue = ConvertToUcs2Format(message, ref datalength);
                    break;
            }
            return string.Format("{0}{1}", datalength.ToString("X2"), returnValue);
        }

        private static string ConvertToSevenBitPack(string message, ref int datacounter)
        {
            var additionalchar = new Dictionary<char, int>() { { '€', 101 }, { '\n', 10 }, { '[', 60 }, { '\\', 47 }, { ']', 62 }, { '^', 20 }, { '{', 40 }, { '|', 64 }, { '}', 41 }, { '~', 61 }, { '@', 0 } };

            var tmpMessageArray = message.ToCharArray();
            var messageArray = new List<int>();

            if (additionalchar.Keys.Any(charx => tmpMessageArray.Any(x => x == charx) || tmpMessageArray.Select(Convert.ToInt32).Any(z => z == 10)))
            {
                foreach (var tmpchar in tmpMessageArray)
                {
                    if (additionalchar.Keys.Any(x => x == tmpchar) || Convert.ToInt32(tmpchar) == 10)
                    {
                        messageArray.Add(27);
                        messageArray.Add(Convert.ToInt32(tmpchar) == 10 ? 10 : additionalchar.First(x => x.Key == tmpchar).Value);
                        continue;
                    }
                    messageArray.Add(tmpchar);
                }
            }
            else
            {
                messageArray = tmpMessageArray.Select(Convert.ToInt32).ToList();
            }

            var binstring = string.Empty;
            foreach (var padded in messageArray.Select(mchar => Convert.ToString(mchar, 2).PadLeft(7, '0')))
            {
                binstring = string.Format("{0}{1}", padded, binstring);
                datacounter++;
            }
            var returnValue = string.Empty;

            for (var index = 0; index < datacounter; index++)
            {
                if (binstring.Length < 1) break;
                if (binstring.Length < 8) binstring = binstring.PadLeft(8, '0');
                returnValue += Convert.ToInt32(new string(binstring.Reverse().Take(8).Reverse().ToArray()), 2).ToString("X2");
                binstring = binstring.Remove(binstring.Length - 8, 8);
            }
            return returnValue;
        }

        private static string ConvertToUcs2Format(string message, ref int datacounter)
        {
            var bitmessage = Encoding.BigEndianUnicode.GetBytes(message);
            datacounter = bitmessage.Length;
            return bitmessage.Aggregate(string.Empty, (current, xchar) => current + Convert.ToString(xchar, 16).PadLeft(2, '0'));
        }

        private static string GenerateUserDataHeader(int reference1, int reference2, int messagecounter, int maxmessage)
        {
            if (maxmessage < 2) return string.Empty;
            var returnValue = string.Empty;
            returnValue = string.Format("{0}060804", returnValue);
            returnValue = string.Format("{0}{1}", returnValue, reference1.ToString("X2"));
            returnValue = string.Format("{0}{1}", returnValue, reference2.ToString("X2"));
            returnValue = string.Format("{0}{1}", returnValue, maxmessage.ToString("X2"));
            returnValue = string.Format("{0}{1}", returnValue, messagecounter.ToString("X2"));
            return returnValue;
        }
    }
}