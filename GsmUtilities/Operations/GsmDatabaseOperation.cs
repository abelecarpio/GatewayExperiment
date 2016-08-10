using GsmManager.DataManagers.GsmDataManagers;
using GsmManager.Entities.GsmEntities;
using GsmUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using GsmManager.DataManagers.SmsDataManagers;
using GsmManager.Entities.SmsEntities;
using GsmUtilities.Models;

namespace GsmUtilities.Operations
{
    internal class GsmDatabaseOperation
    {
        internal ModemDefinition GetDefinedComPort()
        {
            ModemDefinition returnValue = null;
            ManagerModemDefinition manager = null;
            try
            {
                manager = new ManagerModemDefinition();
                var result = manager.GetModemDefinition();
                if (!result.IsSuccess) throw result.CurrentException;
                returnValue = result.OperationResult;
            }
            catch (Exception ex) { ErrorLogHelper<GsmDatabaseOperation>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
            return returnValue;
        }

        internal SystemSetting GetSystemSetting()
        {
            ManagerSystemSetting manager = null;
            try
            {
                manager = new ManagerSystemSetting();
                var result = manager.GetSystemSetting();
                if (!result.IsSuccess) throw result.CurrentException;
                return result.OperationResult;
            }
            catch (Exception ex) { ErrorLogHelper<GsmDatabaseOperation>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
            return null;
        }

        internal List<KeywordBuilder> GetKeywords()
        {
            var returnValue = new List<KeywordBuilder>();
            ManagerKeywordBuilder manager = null;
            try
            {
                manager = new ManagerKeywordBuilder();
                var result = manager.SelectAllKeyword();
                if (!result.IsSuccess) throw result.CurrentException;
                returnValue = new List<KeywordBuilder>(result.OperationResult);
            }
            catch (Exception ex) { ErrorLogHelper<GsmDatabaseOperation>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
            return returnValue;
        }

        internal List<MobilePattern> GetNumberPatterns()
        {
            var returnValue = new List<MobilePattern>();
            ManagerMobilePattern manager = null;
            try
            {
                manager = new ManagerMobilePattern();
                var result = manager.SelectAllPattern();
                if (!result.IsSuccess) throw result.CurrentException;
                returnValue = new List<MobilePattern>(result.OperationResult);
            }
            catch (Exception ex) { ErrorLogHelper<GsmDatabaseOperation>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
            return returnValue;
        }

        internal List<SmsMessage> GetPendingMessages()
        {
            var returnValue = new List<SmsMessage>();
            ManagerOutbox manager = null;
            try
            {
                manager = new ManagerOutbox();
                var result = manager.GetAllPendingMessages();
                if (!result.IsSuccess) throw result.CurrentException;
                var messages = new List<Outbox>(result.OperationResult);
                if (!messages.Any()) return returnValue;
                returnValue.AddRange(messages.Select(message => new SmsMessage()
                {
                    ReferenceId = message.MessageId,
                    MobileNumber = message.MobileNumber,
                    TextMessage = message.SmsMessage,
                    Priority = message.IsUrgent ? MessagePriority.Urgent: MessagePriority.Normal
                }));
            }
            catch (Exception ex) { ErrorLogHelper<GsmDatabaseOperation>.LogError(ex); }
            finally { if (manager != null) manager.Dispose(); }
            return returnValue;
        } 
    }
}