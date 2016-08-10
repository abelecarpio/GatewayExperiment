using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GsmUtilities.Operations
{
    public class DeviceDetection
    {
        #region PRIVATE NATIVE WINDOW

        private sealed class MessageWindow : NativeWindow
        {
            public MessageWindow()
            {
                var cp = new CreateParams { Caption = GetType().FullName };
                // NOTE that you cannot use a "message window" for this broadcast message
                //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                //  cp.Parent = (IntPtr)(-3); // HWND_MESSAGE
                //Debug.WriteLine("Creating MessageWindow " + cp.Caption);
                CreateHandle(cp);
            }

            private const int WM_DESTROY = 0x02;
            private const int WM_DEVICECHANGE = 0x219;

            private enum Dbt
            {
                //A device has been inserted and is now available.
                Devicearrival = 0x8000,

                //Permission to remove a device is requested. Any application can deny this request and cancel the removal.
                Devicequeryremove = 0x8001,

                //Request to remove a device has been canceled.
                Devicequeryremovefailed = 0x8002,

                //Device is about to be removed. Cannot be denied.
                Deviceremovepending = 0x8003,

                //Device has been removed.
                Deviceremovecomplete = 0x8004,

                //Device-specific event.
                Devicetypespecific = 0x8005,

                //User-defined event
                Customevent = 0x8006
            }

            private enum Dbtdevtyp : uint
            {
                //OEM-defined device type
                DbtDevtypOem = 0x00000000,

                //Devnode number
                DbtDevtypDevnode = 0x00000001,

                //Logical volume
                DbtDevtypVolume = 0x00000002,

                //Serial, parallel
                DbtDevtypPort = 0x00000003,

                //Network resource
                DbtDevtypNet = 0x00000004,

                //Device interface class
                DbtDevtypDeviceinterface = 0x00000005,

                //File system handle
                DbtDevtypHandle = 0x00000006
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    // WINDOWS MESSAGE DESTROY
                    case WM_DESTROY:
                        _messageWindow = null;
                        break;

                    // WINDOWS MESSAGE DEVICE CHANGE
                    case WM_DEVICECHANGE:

                        var changeType = (Dbt)m.WParam;
                        var deviceType = (Dbtdevtyp)(m.LParam == IntPtr.Zero ? 0 : Marshal.ReadInt32(m.LParam, 4));

                        //Debug.WriteLine(String.Format("WM_DEVICECHANGE changeType = {0}, deviceType = {1}", changeType, deviceType));

                        if (deviceType != Dbtdevtyp.DbtDevtypPort) break;

                        #region CHANGED TYPE
                        switch (changeType)
                        {
                           
                            #region ARRIVAL TYPE
                            case Dbt.Devicearrival:
                                FirePortArrived(m.LParam);
                                break;
                            case Dbt.Devicequeryremove:
                                FirePortRemovalAttempt(m.LParam);
                                break;
                            case Dbt.Deviceremovepending:
                                FirePortRemovePending(m.LParam);
                                break;
                            case Dbt.Devicequeryremovefailed:
                                FirePortRemoveFailed(m.LParam);
                                break;
                            case Dbt.Deviceremovecomplete:
                                FirePortRemoveCompleted(m.LParam);
                                break;
                            #endregion ARRIVAL TYPE

                            case Dbt.Devicetypespecific:
                            case Dbt.Customevent:
                                break;
                            default:
                                break;
                        }
                        break;
                        #endregion CHANGED TYPE

                    default:
                        break;
                }

                //CALL DEFAULT HANDLER
                base.WndProc(ref m);
            }
        }

        #endregion PRIVATE NATIVE WINDOW

        #region INTERNAL CODES

        private static readonly EventHandlerList Events = new EventHandlerList();
        private static MessageWindow _messageWindow = null;

        private static void AddEvent(object key, Delegate value)
        {
            Events.AddHandler(key, value);
            if (_messageWindow == null) _messageWindow = new MessageWindow();
        }

        private static void RemoveEvent(object key, Delegate value)
        {
            Events.RemoveHandler(key, value);
        }

        #endregion INTERNAL CODES

        #region EVENTS

        #region PORT ARRIVED

        private static readonly object PortArrivedEvent = new object();

        public static event EventHandler<PortChangeEventArgs> PortArrived
        {
            add { AddEvent(PortArrivedEvent, value); }
            remove { RemoveEvent(PortArrivedEvent, value); }
        }

        private static void FirePortArrived(IntPtr lParam)
        {
            var handler = (EventHandler<PortChangeEventArgs>)Events[PortArrivedEvent];
            if (handler == null) return;
            var portName = Marshal.PtrToStringAuto((IntPtr)((long)lParam + 12));
            handler(null, new PortChangeEventArgs(portName));
        }

        #endregion PORT ARRIVED

        #region PORT REMOVAL

        private static readonly object PortRemovalEvent = new object();

        public static event EventHandler<PortChangeEventArgs> PortRemoval
        {
            add { AddEvent(PortRemovalEvent, value); }
            remove { RemoveEvent(PortRemovalEvent, value); }
        }

        private static void FirePortRemovalAttempt(IntPtr lParam)
        {
            var handler = (EventHandler<PortChangeEventArgs>)Events[PortRemovalEvent];
            if (handler == null) return;
            var portName = Marshal.PtrToStringAuto((IntPtr)((long)lParam + 12));
            handler(null, new PortChangeEventArgs(portName));
        }

        #endregion PORT REMOVAL


        #region PORT REMOVAL PENDING

        private static readonly object PortRemovePendingEvent = new object();
        public static event EventHandler<PortChangeEventArgs> PortRemovePending
        {
            add { AddEvent(PortRemovePendingEvent, value); }
            remove { RemoveEvent(PortRemovePendingEvent, value); }
        }
        private static void FirePortRemovePending(IntPtr lParam)
        {
            var handler = (EventHandler<PortChangeEventArgs>)Events[PortRemovePendingEvent];
            if (handler == null) return;
            var portName = Marshal.PtrToStringAuto((IntPtr)((long)lParam + 12));
            handler(null, new PortChangeEventArgs(portName));
        }

        #endregion PORT REMOVAL PENDING


        #region  PORT REMOVAL FAILED

        private static readonly object PortRemoveFailedEvent = new object();
        public static event EventHandler<PortChangeEventArgs> PortRemoveFailed
        {
            add { AddEvent(PortRemoveFailedEvent, value); }
            remove { RemoveEvent(PortRemoveFailedEvent, value); }
        }
        private static void FirePortRemoveFailed(IntPtr lParam)
        {
            var handler = (EventHandler<PortChangeEventArgs>)Events[PortRemoveFailedEvent];
            if (handler == null) return;
            var portName = Marshal.PtrToStringAuto((IntPtr)((long)lParam + 12));
            handler(null, new PortChangeEventArgs(portName));
        }

        #endregion  PORT REMOVAL FAILED


        #region PORT REMOVED COMPLETED

        private static readonly object PortRemoveCompletedEvent = new object();

        public static event EventHandler<PortChangeEventArgs> PortRemoveCompleted
        {
            add { AddEvent(PortRemoveCompletedEvent, value); }
            remove { RemoveEvent(PortRemoveCompletedEvent, value); }
        }
        private static void FirePortRemoveCompleted(IntPtr lParam)
        {
            var handler = (EventHandler<PortChangeEventArgs>)Events[PortRemoveCompletedEvent];
            if (handler == null) return;
            var portName = Marshal.PtrToStringAuto((IntPtr)((long)lParam + 12));
            handler(null, new PortChangeEventArgs(portName));
        }

        #endregion PORT REMOVED COMPLETED

        #endregion EVENTS
    }

    #region EVENT OBJECT

    public sealed class PortChangeEventArgs : EventArgs
    {
        public readonly string Name;

        public PortChangeEventArgs(string name)
        {
            Name = name;
        }
    }

    #endregion EVENT OBJECT
}