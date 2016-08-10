using GsmUtilities;
using GsmUtilities.Models;
using GsmUtilitiesTest.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GsmManager.Entities.GsmEntities;

namespace GsmUtilitiesTest
{
    public partial class FrmMainView : Form
    {
        public FrmMainView()
        {
            InitializeComponent();
            SetRefreshEnableProperty(false);
            SetModemListProperty(false);
        }

        private void FrmMainView_Load(object sender, EventArgs e)
        {
            try
            {
                GetActiveSetting();
                var relay = GsmUtilityRelay.Instance;
                relay.OnActivityChanged = SetActivityValue;
                relay.OnModemRefreshDone = OnModemRefreshDone;
                relay.OnModemStatusChanged = SetModemStatus;
                relay.OnSignalStrengthChanged = SetSignalStrength;
                relay.OnReceivedQueueChanged = OnReceivedQueueChanged;
                relay.OnProcessStateChanged = SetProcessStatus;
                SetControlData();
                relay.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"SMS Gateway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region PRIVATE PROPERTIES

        private SystemSetting ActiveSystemSetting { get; set; }
        private ModemDefinition ActiveModemDefinition { get; set; }

        #endregion PRIVATE PROPERTIES

        #region PRIVATE CALLBACKS

        private void SetActivityValue(string value)
        {
            if (LblActivity.InvokeRequired)
            {
                LblActivity.BeginInvoke((MethodInvoker)delegate() { LblActivity.Text = value; });
                return;
            }
            LblActivity.Text = value;
        }

        private void OnModemRefreshDone()
        {
            SetRefreshEnableProperty(false);
            SetModemListProperty(false);
            BindModemList();
            SetRefreshEnableProperty(true);
            SetModemListProperty(true);
        }

        private void SetModemStatus(ModemStatus value)
        {
            if (LblModemStatus.InvokeRequired)
            {
                LblModemStatus.BeginInvoke((MethodInvoker)delegate()
                {
                    LblModemStatus.Text = (value == ModemStatus.Disconnected) ? "Disconnected" : "Connected";
                });
                return;
            }
            LblModemStatus.Text = (value == ModemStatus.Disconnected) ? "Disconnected" : "Connected";
        }

        private void SetSignalStrength(SignalStrength value)
        {
            if (LblSignal.InvokeRequired)
            {
                LblSignal.BeginInvoke((MethodInvoker)delegate()
                {
                    LblSignal.Text = ((int)value).ToString();
                });
                return;
            }
            LblSignal.Text = ((int)value).ToString();
        }

        private void SetProcessStatus()
        {
            if (LblProcessStatus.InvokeRequired)
            {
                LblProcessStatus.BeginInvoke((MethodInvoker)delegate()
                {
                    LblProcessStatus.Text = GsmUtilityRelay.Instance.CurrentProcessState == ProcessState.Running ? "Running" : "Stopped";
                });
            }
            else
            {
                LblProcessStatus.Text = GsmUtilityRelay.Instance.CurrentProcessState == ProcessState.Running ? "Running" : "Stopped";
            }
        }

        private void OnReceivedQueueChanged()
        {
            if (GsmUtilityRelay.Instance.InboxMessages.IsEmpty) return;
            //var recievemessages = GsmUtilityRelay.Instance.InboxMessages.Values.OrderBy(x=>x.)
        }


        #endregion PRIVATE CALLBACKS

        #region PRIVATE FUNCTIONS

        private void GetActiveSetting()
        {
            var dbops = new DatabaseOperations();
            ActiveModemDefinition = dbops.GetModemDefinition();
            ActiveSystemSetting = dbops.GetSystemSetting();
        }

        private void SetControlData()
        {
            SetSystemSettingControlData();
            SetModemDefinitionControlData();
        }

        private void SetSystemSettingControlData()
        {
            if (ActiveSystemSetting == null) return;
            SetFailedCallback(ActiveSystemSetting.FailedCallback);
            SetReceivedCallback(ActiveSystemSetting.ReceivedCallback);
            SetSentCallback(ActiveSystemSetting.SentCallback);
            SetEnableWebApi(ActiveSystemSetting.WebApiEnable);
            SetWebApiPort(ActiveSystemSetting.WebApiPort ?? 0);
        }

        private void SetModemDefinitionControlData()
        {
            if (ActiveModemDefinition == null) return;
            SetAutoConnect(ActiveModemDefinition.AutoConnect);
            SetWriteFailed(ActiveModemDefinition.LogFailed);
            SetWriteReceived(ActiveModemDefinition.LogReceived);
            SetWriteSent(ActiveModemDefinition.LogSent);
            SetRetryAttempt(ActiveModemDefinition.RetryAttempt);
            SetSendingInterval(ActiveModemDefinition.SendingInterval);
            SetSendingTimeout(ActiveModemDefinition.SendingTimeout);
        }

        #endregion PRIVATE FUNCTIONS

        #region CONTROL SUPPORTS

        private void BindModemList()
        {
            var modemList = new List<ModemPreference>();
            var utilRelay = GsmUtilityRelay.Instance;

            if (utilRelay.AvailableModem.Count > 0) modemList = utilRelay.AvailableModem.ToList();
            modemList.Insert(0, new ModemPreference()
            {
                ComPort = "",
                Manufacturer = "Please Select a Modem"
            });


            if (CbModemList.InvokeRequired)
            {
                CbModemList.BeginInvoke((MethodInvoker)delegate()
                {
                    CbModemList.DataSource = modemList;
                    CbModemList.DisplayMember = "FriendlyName";
                    CbModemList.ValueMember = "ComPort";
                    if (ActiveModemDefinition != null
                        && !string.IsNullOrEmpty(ActiveModemDefinition.ComPort)
                        && modemList.Any(x => x.ComPort == ActiveModemDefinition.ComPort))
                        CbModemList.SelectedValue = ActiveModemDefinition.ComPort;
                });
            }
            else
            {
                CbModemList.DataSource = modemList;
                CbModemList.DisplayMember = "FriendlyName";
                CbModemList.ValueMember = "ComPort";
                if (ActiveModemDefinition != null
                        && !string.IsNullOrEmpty(ActiveModemDefinition.ComPort)
                        && modemList.Any(x => x.ComPort == ActiveModemDefinition.ComPort))
                    CbModemList.SelectedValue = ActiveModemDefinition.ComPort;
            }

        }

        private void SetRefreshEnableProperty(bool isenable)
        {
            if (BtnRefreshModem.InvokeRequired)
                BtnRefreshModem.BeginInvoke((MethodInvoker)delegate() { BtnRefreshModem.Enabled = isenable; });
            else BtnRefreshModem.Enabled = isenable;
        }

        private void SetModemListProperty(bool isenable)
        {
            if (CbModemList.InvokeRequired)
                CbModemList.BeginInvoke((MethodInvoker)delegate() { CbModemList.Enabled = isenable; });
            else CbModemList.Enabled = isenable;
        }

        private void BtnRefreshModem_Click(object sender, EventArgs e)
        {
            SetRefreshEnableProperty(false);
            SetModemListProperty(false);
            GsmUtilityRelay.Instance.RefreshModemList();
        }

        #region CALLBACKS PATHS

        private void SetFailedCallback(string value)
        {
            if (LblFailedCallback.InvokeRequired)
            {
                LblFailedCallback.BeginInvoke((MethodInvoker)delegate() { LblFailedCallback.Text = value; });
                return;
            }
            LblFailedCallback.Text = value;
        }

        private void SetSentCallback(string value)
        {
            if (LblSentCallback.InvokeRequired)
            {
                LblSentCallback.BeginInvoke((MethodInvoker)delegate() { LblSentCallback.Text = value; });
                return;
            }
            LblSentCallback.Text = value;
        }

        private void SetReceivedCallback(string value)
        {
            if (LblReceivedCallback.InvokeRequired)
            {
                LblReceivedCallback.BeginInvoke((MethodInvoker)delegate() { LblReceivedCallback.Text = value; });
                return;
            }
            LblReceivedCallback.Text = value;
        }

        #endregion CALLBACKS PATHS

        #region LOGGING

        private void SetWriteFailed(bool value)
        {
            if (CbWriteFailed.InvokeRequired)
            {
                CbWriteFailed.BeginInvoke((MethodInvoker)delegate() { CbWriteFailed.Checked = value; });
                return;
            }
            CbWriteFailed.Checked = value;
        }

        private void SetWriteSent(bool value)
        {
            if (CbWriteSent.InvokeRequired)
            {
                CbWriteSent.BeginInvoke((MethodInvoker)delegate() { CbWriteSent.Checked = value; });
                return;
            }
            CbWriteSent.Checked = value;
        }

        private void SetWriteReceived(bool value)
        {
            if (CbWriteReceived.InvokeRequired)
            {
                CbWriteReceived.BeginInvoke((MethodInvoker)delegate() { CbWriteReceived.Checked = value; });
                return;
            }
            CbWriteReceived.Checked = value;
        }

        #endregion LOGGING

        #region COMMON SETTINGS

        private void SetWebApiPort(int value)
        {
            if (NudWebApiPort.InvokeRequired)
            {
                NudWebApiPort.BeginInvoke((MethodInvoker)delegate() { NudWebApiPort.Value = value; });
                return;
            }
            NudWebApiPort.Value = value;
        }

        private void SetEnableWebApi(bool value)
        {
            if (CbEnableWebApi.InvokeRequired)
            {
                CbEnableWebApi.BeginInvoke((MethodInvoker)delegate() { CbEnableWebApi.Checked = value; });
                return;
            }
            CbEnableWebApi.Checked = value;
        }

        private void SetSendingInterval(int value)
        {
            if (NudSendingInterval.InvokeRequired)
            {
                NudSendingInterval.BeginInvoke((MethodInvoker)delegate() { NudSendingInterval.Value = value; });
                return;
            }
            NudSendingInterval.Value = value;
        }

        private void SetSendingTimeout(int value)
        {
            if (NudSendingTimeout.InvokeRequired)
            {
                NudSendingTimeout.BeginInvoke((MethodInvoker)delegate() { NudSendingTimeout.Value = value; });
                return;
            }
            NudSendingTimeout.Value = value;
        }

        private void SetRetryAttempt(int value)
        {
            if (NudRetryAttempt.InvokeRequired)
            {
                NudRetryAttempt.BeginInvoke((MethodInvoker)delegate() { NudRetryAttempt.Value = value; });
                return;
            }
            NudRetryAttempt.Value = value;
        }

        #endregion COMMON SETTINGS

        #region MODEM SETTING

        private void SetAutoConnect(bool value)
        {
            if (CbReconnect.InvokeRequired)
            {
                CbReconnect.BeginInvoke((MethodInvoker)delegate() { CbReconnect.Checked = value; });
                return;
            }
            CbReconnect.Checked = value;
        }

        #endregion MODEM SETTING

        #region BROWSE CONTROLS

        private void BtnBrowseReceived_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SetReceivedCallback(dialog.FileName);
        }

        private void BtnBrowseSent_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SetSentCallback(dialog.FileName);
        }

        private void BtnBrowseFailed_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SetFailedCallback(dialog.FileName);
        }

        #endregion BROWSE CONTROLS

        #region CLEAR CONTROL

        private void BtnBrowseReceivedClear_Click(object sender, EventArgs e)
        {
            SetReceivedCallback("");
        }

        private void BtnBrowseSentClear_Click(object sender, EventArgs e)
        {
            SetSentCallback("");
        }

        private void BtnBrowseFailedClear_Click(object sender, EventArgs e)
        {
            SetFailedCallback("");
        }

        #endregion CLEAR CONTROL

        private void BtnSaveSetting_Click(object sender, EventArgs e)
        {
            try
            {
                BtnSaveSetting.Enabled = false;
                if (CbModemList.SelectedValue == null ||
                    string.IsNullOrEmpty(CbModemList.SelectedValue.ToString()))
                    throw new Exception("Please select a valid modem.");

                //TODO: VALIDATION

                //SYSTEM SETTING
                ActiveSystemSetting = ActiveSystemSetting ?? new SystemSetting();
                ActiveSystemSetting.FailedCallback = LblFailedCallback.Text;
                ActiveSystemSetting.ReceivedCallback = LblReceivedCallback.Text;
                ActiveSystemSetting.SentCallback = LblSentCallback.Text;
                ActiveSystemSetting.WebApiEnable = CbEnableWebApi.Checked;
                ActiveSystemSetting.WebApiPort = (int)NudWebApiPort.Value;

                //MODEM DEFINITION
                var selectedcomport = CbModemList.SelectedValue.ToString();
                var selectedmodem = GsmUtilityRelay.Instance.AvailableModem
                    .FirstOrDefault(x => x.ComPort == selectedcomport);
                if (selectedmodem == null) throw new Exception("Unable to find selected modem.");

                ActiveModemDefinition = ActiveModemDefinition ?? new ModemDefinition();
                ActiveModemDefinition.ComPort = selectedmodem.ComPort;
                ActiveModemDefinition.AutoConnect = CbReconnect.Checked;
                ActiveModemDefinition.BaudRate = selectedmodem.BaudRate;
                ActiveModemDefinition.Imei = selectedmodem.Imei;
                ActiveModemDefinition.LogFailed = CbWriteFailed.Checked;
                ActiveModemDefinition.LogReceived = CbWriteReceived.Checked;
                ActiveModemDefinition.LogSent = CbWriteSent.Checked;
                ActiveModemDefinition.ModemName = selectedmodem.FriendlyName;
                ActiveModemDefinition.RetryAttempt = (int)NudRetryAttempt.Value;
                ActiveModemDefinition.SendingInterval = (int)NudSendingInterval.Value;
                ActiveModemDefinition.SendingTimeout = (int)NudSendingTimeout.Value;

                var dbops = new DatabaseOperations();
                dbops.SaveConfiguration(ActiveModemDefinition, ActiveSystemSetting);
                GsmUtilityRelay.Instance.UpdateSettings(ActiveSystemSetting, ActiveModemDefinition);

                MessageBox.Show(@"Configuration has been saved.", @"SMS Gateway", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"SMS Gateway", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BtnSaveSetting.Enabled = true;
            }
        }

        #endregion CONTROL SUPPORTS

        private void BtnSend_Click(object sender, EventArgs e)
        {
            GsmUtilityRelay.Instance.SendMessage(new SmsMessage()
            {
                MobileNumber = TxtMobileNumber.Text,
                TextMessage = RtbMessage.Text,
                Priority = MessagePriority.Urgent
            });
        }

        private void BtnProcessStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (GsmUtilityRelay.Instance.CurrentProcessState == ProcessState.Running)
                    GsmUtilityRelay.Instance.StopProcess();
                else GsmUtilityRelay.Instance.StartProcess();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}