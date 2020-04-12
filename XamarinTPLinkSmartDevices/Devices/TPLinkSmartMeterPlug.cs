using System;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartMeterPlug : TPLinkSmartPlug
    {
        public TPLinkSmartMeterPlug(string hostname) : base(hostname)
        {
            Task.Run(async() =>
            {
                CurrentPowerUsage =  await Execute("emeter", "get_realtime");
            });
        }

        public PowerData CurrentPowerUsage { get; private set; }

    }
}