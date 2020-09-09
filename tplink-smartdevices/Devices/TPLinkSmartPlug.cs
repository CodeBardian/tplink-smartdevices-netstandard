using System;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartPlug : TPLinkSmartDevice
    {
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

        public TPLinkSmartPlug(string hostname, int port = 9999) : base(hostname, port)
        {
            Task.Run(async () => await Refresh().ConfigureAwait(false)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await ExecuteAsync("system", "get_sysinfo");
            Features = ((string)sysInfo.feature).Split(':');
            LedOn = !(bool)sysInfo.led_off;
            PoweredOnSince = (int)sysInfo.on_time == 0 ? default : DateTime.Now - TimeSpan.FromSeconds((int)sysInfo.on_time);

            await Refresh(sysInfo);
        }

        /// <summary>
        /// Send command which changes power state to plug
        /// </summary>
        public async Task SetOutletPoweredAsync(bool value)
        {
            await ExecuteAsync("system", "set_relay_state", "state", value ? 1 : 0);
            OutletPowered = value;
        }

        /// <summary>
        /// Send command which enables or disables night mode (LED state)
        /// </summary>
        public async Task SetLedOnAsync(bool value)
        {
            await ExecuteAsync("system", "set_led_off", "off", value ? 0 : 1);
            LedOn = value;
        }
    }
}