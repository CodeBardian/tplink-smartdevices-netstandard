using System;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartPlug : TPLinkSmartDevice
    {
        private bool _powered;

        /// <summary>
        /// If the outlet relay is powered on
        /// </summary>
        public bool OutletPowered { get; private set; }

        /// <summary>
        /// If the LED on the smart plug is on
        /// </summary>
        public bool LedOn { get; private set; }

        /// <summary>
        /// DateTime the relay was powered on
        /// </summary>
        public DateTime PoweredOnSince { get; private set; }

        public string[] Features { get; private set; }

        public TPLinkSmartPlug(string hostname, int port=9999) : base(hostname,port)
        {
            Task.Run(() => Refresh()).Wait();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo");
            Features = ((string)sysInfo.feature).Split(':');
            _powered = (bool)sysInfo.relay_state;
            LedOn = !(bool)sysInfo.led_off;
            if ((int)sysInfo.on_time == 0)
                PoweredOnSince = default(DateTime);
            else
                PoweredOnSince = DateTime.Now - TimeSpan.FromSeconds((int)sysInfo.on_time);

            await Refresh(sysInfo);
        }

        /// <summary>
        /// Send command which changes power state to bulb
        /// </summary>
        public void SetOutletPowered(bool value)
        {
            Task.Run(async() =>
            {
                await Execute("system", "set_relay_state", "state", value ? 1 : 0);
                this.OutletPowered = value;
            });
        }
    }
}