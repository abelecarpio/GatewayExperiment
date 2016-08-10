using System;
using GsmManager.Entities.GsmEntities;
using GsmUtilitiesTest.DatabaseManagers;

namespace GsmUtilitiesTest.Operations
{
    internal class DatabaseOperations
    {
        internal ModemDefinition GetModemDefinition()
        {
            ManagerModemDefinition manager = null;
            try
            {
                manager = new ManagerModemDefinition();
                var result = manager.GetModemDefinition();
                if (!result.IsSuccess) throw result.CurrentException;
                return result.OperationResult;
            }
            finally { if (manager != null) manager.Dispose(); }
            return null;
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
            finally { if (manager != null) manager.Dispose(); }
            return null;
        }


        internal void SaveConfiguration(ModemDefinition modem, SystemSetting setting)
        {
            ManagerSystemSetting settingManager = null;
            try
            {
                settingManager = new ManagerSystemSetting();
                var settingresult = settingManager.SaveSystemSettings(setting);
                if (!settingresult.IsSuccess) throw settingresult.CurrentException;
            }
            finally { if (settingManager != null) settingManager.Dispose(); }


            ManagerModemDefinition modemManager = null;
            try
            {
                modemManager = new ManagerModemDefinition();
                var modemresult = modemManager.SaveModemConfiguration(modem);
                if (!modemresult.IsSuccess) throw modemresult.CurrentException;
            }
            finally { if (modemManager != null) modemManager.Dispose(); }
        }

    }
}