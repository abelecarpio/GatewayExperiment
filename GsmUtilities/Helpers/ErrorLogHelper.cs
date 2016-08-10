using log4net;
using System;

namespace GsmUtilities.Helpers
{
    internal static class ErrorLogHelper<T>
    {
        //private static readonly ILog ExceptionLog = LogManager.GetLogger(typeof(T));
        private static readonly ILog ExceptionLog = LogManager.GetLogger("errorlog");
        internal static void LogError(Exception exception)
        {
            if (exception == null) return;
            ExceptionLog.Error(string.Format("[ERROR] {0}", exception));
        }
    }
}