using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace GsmUtilities.Helpers
{
    public class SerialPortHelper : IDisposable
    {
        public SerialPortHelper(string comPort, int writeTimeout = 0, int readTimeout = 0)
        {
            ShadowSerialIoPort = new SerialPort(comPort)
            {
                Encoding = Encoding.UTF8,
                DtrEnable = true,
                RtsEnable = true,
                WriteTimeout = (writeTimeout > 500) ? writeTimeout : 500,
                ReadTimeout = (readTimeout > 500) ? readTimeout : 500,
                Handshake = Handshake.None
            };
        }

        public string Execute(string command, string expectedResult, int timeout = 5000, bool raiseerror = true)
        {
            if (string.IsNullOrEmpty(command)) throw new ArgumentException("Command should not be empty", command);
            if (ShadowSerialIoPort == null) throw new IOException("COM Port is undefined.");

            IssuedCommand = command;
            ExpectedResult = expectedResult;
            CommandTimeout = timeout;

            if (!ShadowSerialIoPort.IsOpen) ShadowSerialIoPort.Open();
            ResetFlags();

            ShadowSerialIoPort.Write(command);
            ShadowSerialIoPort.Write(((char)26).ToString());

            while (ShadowSerialIoPort.BytesToRead < 1)
            {
                Thread.Sleep(1);
                if (DateTime.Now < CommandEnd) continue;
                HasError = true;
                if (raiseerror)
                    throw new Exception(string.Format("Command Timeout, no response from {0}", ShadowSerialIoPort.PortName));
                break;
            }
            if (HasError) return CommandResult;

            CommandResult = ShadowSerialIoPort.ReadExisting();
            ResetFlags();

            while (!CommandResult.Contains(expectedResult) && !CommandResult.Contains("ERROR"))
            {
                Thread.Sleep(1);
                if (DateTime.Now < CommandEnd) continue;
                if (raiseerror)
                    throw new Exception(string.Format("Command Timeout, no response from {0}", ShadowSerialIoPort.PortName));
                break;
            }
            return CommandResult;
        }


        public string ExecuteCommand(string command, string expectedResult, int timeout = 5000, bool raiseerror = true)
        {
            if (string.IsNullOrEmpty(command)) throw new ArgumentException("Command should not be empty", command);
            if (ShadowSerialIoPort == null) throw new IOException("COM Port is undefined.");

            IssuedCommand = command;
            ExpectedResult = expectedResult;
            CommandTimeout = timeout;

            if (!ShadowSerialIoPort.IsOpen)
            {
                ShadowSerialIoPort.Open();
                ShadowSerialIoPort.DataReceived += ComPortOnDataReceived;
                ShadowSerialIoPort.ErrorReceived += ComPortOnErrorReceived;
            }
            ResetFlags();

            ShadowSerialIoPort.WriteLine(command);

            if (string.IsNullOrEmpty(expectedResult)) return string.Empty;

            while (!IsEndOfResponse && !HasError)
            {
                Thread.Sleep(1);
                if (DateTime.Now < CommandEnd) continue;
                HasError = true;
                AddToResult(string.Format("Command Timeout, no response from {0}", ShadowSerialIoPort.PortName));
            }
            if (HasError && raiseerror) throw new Exception(CommandResult);
            return ((string.IsNullOrEmpty(CommandResult)) ? string.Empty : CommandResult.Trim());
        }

        private void ResetFlags()
        {
            ShadowSerialIoPort.WriteTimeout = CommandTimeout;
            ShadowSerialIoPort.ReadTimeout = CommandTimeout;
            HasError = false;
            CommandResult = string.Empty;
            IsEndOfResponse = false;
            CommandEnd = CommandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(CommandTimeout);
        }

        private void AddToResult(string message)
        {
            if (CommandResult == null) CommandResult = string.Empty;
            CommandResult = string.Format("{0} {1}", CommandResult, message);
        }

        #region CALLBACKS

        private void ComPortOnErrorReceived(object sender, SerialErrorReceivedEventArgs args)
        {
            CommandEnd = DateTime.MaxValue;
            HasError = true;
            AddToResult(((args == null) ? string.Empty : args.ToString()).Replace(IssuedCommand, ""));
            CommandEnd = CommandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(CommandTimeout);
        }

        private void ComPortOnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            CommandEnd = DateTime.MaxValue;
            if (sender == null)
            {
                HasError = true;
                AddToResult("Unable to get Serial Port raised Data Received Event.");
                return;
            }
            var privateserial = (SerialPort)sender;
            if (!privateserial.IsOpen)
            {
                HasError = true;
                AddToResult("Unable to get response on closed serial port.");
                return;
            }
            switch (args.EventType)
            {
                case SerialData.Chars:
                    AddToResult(privateserial.ReadExisting().Replace(IssuedCommand, ""));
                    break;

                case SerialData.Eof:
                    AddToResult(privateserial.ReadExisting().Replace(IssuedCommand, ""));
                    IsEndOfResponse = true;
                    break;

                default:
                    break;
            }
            if (CommandResult.ToUpper().Contains("ERROR")) HasError = true;
            if (CommandResult.Contains(ExpectedResult)) IsEndOfResponse = true;
            CommandEnd = CommandTimeout < 1 ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(CommandTimeout);
        }

        #endregion CALLBACKS

        #region PRIVATE PROPERTIES

        private bool HasError { get; set; }
        private string ExpectedResult { get; set; }
        private string IssuedCommand { get; set; }
        private bool IsEndOfResponse { get; set; }
        internal string CommandResult { get; set; }
        internal int CommandTimeout { get; set; }
        internal DateTime CommandEnd { get; set; }
        private SerialPort ShadowSerialIoPort { get; set; }
        internal SerialPort SerialIoPort { get { return ShadowSerialIoPort; } }
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
                if (ShadowSerialIoPort != null)
                {
                    ShadowSerialIoPort.DataReceived -= ComPortOnDataReceived;
                    ShadowSerialIoPort.ErrorReceived -= ComPortOnErrorReceived;
                    if (ShadowSerialIoPort.IsOpen) ShadowSerialIoPort.Close();
                    ShadowSerialIoPort.Dispose();
                }
            }
            _disposed = true;
        }

        #endregion DISPOSAL
    }
}