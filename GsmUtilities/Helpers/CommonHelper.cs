using System;
using System.Globalization;

namespace GsmUtilities.Helpers
{
    internal static class CommonHelpers
    {
        internal static bool IsNumeric(string inputData)
        {
            var returnValue = !String.IsNullOrWhiteSpace(inputData);
            foreach (var item in inputData.ToCharArray())
            {
                var outref = 0;
                if (Int32.TryParse(item.ToString(CultureInfo.InvariantCulture), out outref)) continue;
                returnValue = false;
                break;
            }
            return returnValue;
        }
    }


    internal static class LogPrefixExtension
    {
        internal static string AddPrefixWriteTimestamp(this string input)
        {
            return string.Format("[WD : {0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), input);
        }

        internal static string AddPrefixReadTimestamp(this string input)
        {
            return string.Format("[RD : {0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), input);
        }
    }

}