using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Task.Run(() => Refresh()).GetAwaiter().GetResult();
        }

        protected TPLinkSmartPlug() { }

        public static async Task<TPLinkSmartPlug> Create(string hostname, int port = 9999)
        {
            var p = new TPLinkSmartPlug() { Hostname = hostname, Port = port };
            await p.Refresh().ConfigureAwait(false);
            return p;
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);

            JObject info = JObject.Parse(Convert.ToString(sysInfo));
            bool hasChildren = info["children"] != null;

            if (hasChildren) throw new Exception("this plug has multiple outlets. use TPLinkSmartMultiPlug instead!");

            OutletPowered = (int)sysInfo.relay_state == 1;
            if ((int)sysInfo.on_time == 0)
                PoweredOnSince = default(DateTime);
            else
                PoweredOnSince = DateTime.Now - TimeSpan.FromSeconds((int)sysInfo.on_time);

            Features = ((string)sysInfo.feature).Split(':');
            LedOn = !(bool)sysInfo.led_off;

            await Refresh((object)sysInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Send command which changes power state to plug
        /// </summary>
        public void SetOutletPowered(bool value)
        {
            Task.Run(async () =>
            {
                await Execute("system", "set_relay_state", "state", value ? 1 : 0).ConfigureAwait(false);
                this.OutletPowered = value;
            });
        }

        private object GetPlugID(int outletId)
        {
            return JArray.FromObject(new [] {$"{DeviceId}0{outletId}"});
        }

        /// <summary>
        /// Send command which enables or disables night mode (LED state)
        /// </summary>
        public void SetLedOn(bool value)
        {
            Task.Run(async () =>
            {
                await Execute("system", "set_led_off", "off", value ? 0 : 1).ConfigureAwait(false);
                this.LedOn = value;
            });
        }
    }
}