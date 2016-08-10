using System;
using System.IO;
using System.Reflection;
using System.Text;
using GsmUtilities.Models;
using log4net;

namespace GsmUtilities.Helpers
{
    internal static class CommunicationLogHelper
    {
        private static readonly ILog ComLog = LogManager.GetLogger("comlog");
        internal static void LogInformation(string message)
            {
                if (message == null) return;
                ComLog.Info(message);
            }

        internal static void LogSentMessage(SmsMessage message, ModemPreference activeModem)
        {
            var logfolder = Path.Combine(OutboxFolder, activeModem.FriendlyName);
            if (!Directory.Exists(logfolder)) Directory.CreateDirectory(logfolder);

            var filename = string.Format("{0}{1}.txt", 
                message.MobileNumber, 
                DateTime.Now.ToString("yyyyMMddhhmmssfffff"));
            File.AppendAllText(Path.Combine(logfolder, filename), message.TextMessage);
        }

        internal static void LogReceiveMessage(SmsMessage message, ModemPreference activeModem)
        {
            var logfolder = Path.Combine(OutboxFolder, activeModem.FriendlyName);
            if (!Directory.Exists(logfolder)) Directory.CreateDirectory(logfolder);

            var filename = string.Format("{0}{1}.txt",
               message.MobileNumber,
               DateTime.Now.ToString("yyyyMMddhhmmssfffff"));
            File.AppendAllText(Path.Combine(logfolder, filename), message.TextMessage);
        }
        

        private static readonly string RootFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

        private static readonly string LogFolder = Path.Combine(RootFolder, "Communication Logs");

        private static readonly string LogFile = Path.Combine(LogFolder,
            string.Format("{0}.log", DateTime.Now.ToString("yyyyMMdd")));
        

        private static readonly string InboxFolder = Path.Combine(RootFolder, "INBOX");
        private static readonly string OutboxFolder = Path.Combine(RootFolder, "OUTBOX");
        
        //private static CommunicationLogHelper _instance = null;
        //private static readonly object Padlock = new object();
    }
}