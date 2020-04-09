using System;
using System.Collections.Generic;
using System.Text;
using TPLinkSmartDevices.Devices;

namespace TPLinkSmartDevices.Events
{
    public class DeviceFoundEventArgs : EventArgs
    {
        public TPLinkSmartDevice Device;

        public DeviceFoundEventArgs(TPLinkSmartDevice device)
        {
            Device = device;
        }
    }
}
