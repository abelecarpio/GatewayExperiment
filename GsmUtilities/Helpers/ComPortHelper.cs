using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace GsmUtilities.Helpers
{
    internal class ComPortHelper : IDisposable
    {
        #region CONSTRUCTOR

        internal ComPortHelper(string comPort, int writeTimeout = 0, int readTimeout = 0)
        {
            ComPort = new SerialPort(comPort)
            {
                Encoding = Encoding.UTF8,
                DtrEnable = true,
                RtsEnable = true
            };
            if (writeTimeout > 0) ComPort.WriteTimeout = writeTimeout;
            if (readTimeout > 0) ComPort.ReadTimeout = readTimeout;
        }

        #endregion CONSTRUCTOR

        #region EXECUTION

        internal string Execute(string command, string expectedResult, bool isRespondAtEnd = true, int commandTimeout = 5000, bool raiseError = true)
        {
            if (string.IsNullOrEmpty(command)) throw new ArgumentException("Command should not be empty", command);
            if (ComPort == null) throw new IOException("COM Port is undefined.");
            IsExpectedResponseAtEnd = isRespondAtEnd;
            InputCommand = command;
            CommTrace = command;
            if (!ComPort.IsOpen)
            {
                ComPort.Open();
                ComPort.DataReceived += ComPortOnDataReceived;
                ComPort.ErrorReceived += ComPortOnErrorReceived;
            }

            Initialize();
            _expectedResult = expectedResult;
            CommandTimeout = commandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(commandTimeout);

            ComPort.Write(command);

            if (string.IsNullOrEmpty(expectedResult)) return string.Empty;


            while (!IsEndOfResponse && !HasError)
            {
                if (DateTime.Now >= CommandTimeout)
                    throw new TimeoutException(string.Format("Command Timeout, no response from {0}", ComPort.PortName));

                Thread.Sleep(1);
            }
            if (HasError && raiseError) throw new Exception(TmpResponse);
            TmpResponse = string.IsNullOrEmpty(TmpResponse) ? string.Empty : TmpResponse.TrimStart();
            TmpResponse = TmpResponse.StartsWith("\r\n") ? TmpResponse.Substring(3) : TmpResponse;
            return TmpResponse;
        }

        #endregion EXECUTION

        #region CALLBACKS

        private void ComPortOnErrorReceived(object sender, SerialErrorReceivedEventArgs args)
        {
            TmpResponse = args.ToString();
            HasError = true;
            TmpResponse = TmpResponse.Replace(CommTrace, "");
        }

        private void ComPortOnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            var privateserial = (SerialPort)sender;
            if (privateserial == null || !privateserial.IsOpen) return;
            switch (args.EventType)
            {
                case SerialData.Chars:
                    TmpResponse = string.Format("{0} {1}", TmpResponse, privateserial.ReadExisting());
                    break;

                case SerialData.Eof:
                    TmpResponse = string.Format("{0} {1}", TmpResponse, privateserial.ReadExisting());
                    IsEndOfResponse = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            TmpResponse = TmpResponse.Replace(CommTrace, "");
            if (string.IsNullOrEmpty(TmpResponse)) return;
            if (TmpResponse.ToUpper().Contains("ERROR"))
            {
                HasError = true;
                IsEndOfResponse = true;
            }

            switch (IsExpectedResponseAtEnd)
            {
                case true:
                    if (TmpResponse.Trim().EndsWith(_expectedResult)) IsEndOfResponse = true;
                    break;
                default:
                    if (TmpResponse.Trim().StartsWith(_expectedResult)) IsEndOfResponse = true;
                    break;
            }
        }

        #endregion CALLBACKS

        #region PRIVATE FUNCTIONS

        private void Initialize()
        {
            IsEndOfResponse = false;
            TmpResponse = string.Empty;
            HasError = false;
        }

        #endregion PRIVATE FUNCTIONS

        #region PRIVATE PROPERTIES

        internal readonly SerialPort ComPort;
        private string _expectedResult;
        private bool IsEndOfResponse { get; set; }
        private string TmpResponse { get; set; }
        private bool HasError { get; set; }
        private string InputCommand { get; set; }
        private string CommTrace { get; set; }
        private DateTime CommandTimeout { get; set; }

        private bool IsExpectedResponseAtEnd { get; set; }

        #endregion PRIVATE PROPERTIES

        #region DISPOSAL

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (ComPort != null)
                {
                    ComPort.DataReceived -= ComPortOnDataReceived;
                    ComPort.ErrorReceived -= ComPortOnErrorReceived;
                    if (ComPort.IsOpen) ComPort.Close();
                    ComPort.Dispose();
                }
            }
            _disposed = true;
        }

        #endregion DISPOSAL
    }
}