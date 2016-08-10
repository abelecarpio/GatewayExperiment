using System;
using System.Diagnostics;

namespace GsmUtilities
{
    [DebuggerDisplay("Status : {IsSuccess}")]
    public class OperationResult<T>
    {

        internal bool ProcessStatusIsSuccess = true;
        private string ProcessStatusExceptionMessage { get; set; }
        private string ProcessStatusExceptionInnerMessage { get; set; }
        private string ProcessStatusExceptionStackTrace { get; set; }
        private string ProcessStatusExceptionInnerStackTrace { get; set; }
        private Exception ProcessStatusTransactionException { get; set; }
        private string ProcessStatusFriendlyMessage { get; set; }
        internal T ProcessStatusOperationResult { get; set; }

        public bool IsSuccess { get { return ProcessStatusIsSuccess; } }
        public string ExceptionMessage { get { return ProcessStatusExceptionMessage; } }
        public string ExceptionInnerMessage { get { return ProcessStatusExceptionInnerMessage; } }
        public string ExceptionStackTrace { get { return ProcessStatusExceptionStackTrace; } }
        public string ExceptionInnerStackTrace { get { return ProcessStatusExceptionInnerStackTrace; } }
        public Exception TransactionException { get { return ProcessStatusTransactionException; } }
        public string FriendlyMessage { get { return ProcessStatusFriendlyMessage; } }
        public T ProcessResult { get { return ProcessStatusOperationResult; } }

        internal void CreateFromException(Exception ex, T result, string prefixMessage = "", string suffixMessage = "")
        {
            ProcessStatusIsSuccess = false;
            if (ex == null) return;
            ProcessStatusOperationResult = result;
            ProcessStatusTransactionException = ex;
            ProcessStatusExceptionMessage = string.Format("{0} {1} {2}", prefixMessage, ex.ToString(), suffixMessage);
            ProcessStatusExceptionStackTrace = ex.StackTrace;
            ProcessStatusExceptionInnerMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
            ProcessStatusExceptionInnerStackTrace = ex.InnerException != null ? ex.InnerException.StackTrace : null;
        }

        internal void CreateFromException(Exception ex, string prefixMessage = "", string suffixMessage = "")
        {
            ProcessStatusIsSuccess = false;
            if (ex == null) return;

            ProcessStatusTransactionException = ex;
            ProcessStatusExceptionMessage = string.Format("{0} {1} {2}", prefixMessage, ex.ToString(), suffixMessage);
            ProcessStatusExceptionStackTrace = ex.StackTrace;
            ProcessStatusExceptionInnerMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
            ProcessStatusExceptionInnerStackTrace = ex.InnerException != null ? ex.InnerException.StackTrace : null;
        }

        internal void SetFriendlyMessage(string message)
        {
            ProcessStatusFriendlyMessage = message;
        }

    }





    [DebuggerDisplay("Status : {IsSuccess}")]
    internal class OperationResult
    {
        internal bool ProcessStatusIsSuccess = true;
        private string ProcessStatusExceptionMessage { get; set; }
        private string ProcessStatusExceptionInnerMessage { get; set; }
        private string ProcessStatusExceptionStackTrace { get; set; }
        private string ProcessStatusExceptionInnerStackTrace { get; set; }
        private Exception ProcessStatusTransactionException { get; set; }
        private string ProcessStatusFriendlyMessage { get; set; }


        public bool IsSuccess { get { return ProcessStatusIsSuccess; } }
        public string ExceptionMessage { get { return ProcessStatusExceptionMessage; } }
        public string ExceptionInnerMessage { get { return ProcessStatusExceptionInnerMessage; } }
        public string ExceptionStackTrace { get { return ProcessStatusExceptionStackTrace; } }
        public string ExceptionInnerStackTrace { get { return ProcessStatusExceptionInnerStackTrace; } }
        public Exception TransactionException { get { return ProcessStatusTransactionException; } }
        public string FriendlyMessage { get { return ProcessStatusFriendlyMessage; } }


        internal void CreateFromException(Exception ex, string prefixMessage = "", string suffixMessage = "")
        {
            ProcessStatusIsSuccess = false;
            if (ex == null) return;
            ProcessStatusTransactionException = ex;
            ProcessStatusExceptionMessage = string.Format("{0} {1} {2}", prefixMessage, ex.ToString(), suffixMessage);
            ProcessStatusExceptionStackTrace = ex.StackTrace;
            ProcessStatusExceptionInnerMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
            ProcessStatusExceptionInnerStackTrace = ex.InnerException != null ? ex.InnerException.StackTrace : null;
        }

        internal void SetFriendlyMessage(string message)
        {
            ProcessStatusFriendlyMessage = message;
        }

    }
}
