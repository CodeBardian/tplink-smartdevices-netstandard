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

        public int OutletCount { get; private set; }

        public Outlet[] Outlets { get; private set; }

        public TPLinkSmartPlug(string hostname, int port = 9999) : base(hostname, port)
        {
            Task.Run(() => Refresh()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo");

            JObject info = JObject.Parse(Convert.ToString(sysInfo));
            bool hasChildren = info["children"] != null;

            if (hasChildren)
            {
                OutletCount = (int)sysInfo.child_num;
                Outlets = JsonConvert.DeserializeObject<List<Outlet>>(Convert.ToString(sysInfo.children)).ToArray();
            }
            else
            {
                OutletCount = 1;
                OutletPowered = (int)sysInfo.relay_state == 1;
                if ((int)sysInfo.on_time == 0)
                    PoweredOnSince = default(DateTime);
                else
                    PoweredOnSince = DateTime.Now - TimeSpan.FromSeconds((int)sysInfo.on_time);
            }

            Features = ((string)sysInfo.feature).Split(':');
            LedOn = !(bool)sysInfo.led_off;

            await Refresh(sysInfo);
        }

        /// <summary>
        /// Send command which changes power state to plug
        /// </summary>
        public void SetOutletPowered(bool value, int outletId = -1)
        {
            if (outletId > OutletCount - 1) throw new ArgumentException("Plug does not have a outlet with specified id");

            Task.Run(async () =>
            {
                //toggle all outlets of plug
                if (OutletCount == 1 || outletId == -1)
                {
                    await Execute("system", "set_relay_state", "state", value ? 1 : 0);
                    this.OutletPowered = value;
                }
                //toggle specific outlet
                else
                {
                    JObject root = new JObject { new JProperty("context", new JObject { new JProperty("child_ids", GetPlugID(outletId)) }), 
                        new JProperty("system", new JObject { new JProperty("set_relay_state", new JObject { new JProperty("state", value ? 1 : 0) }) }) };

                    string message = root.ToString(Formatting.None);
                    await Execute(message);
                }
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
                await Execute("system", "set_led_off", "off", value ? 0 : 1);
                this.LedOn = value;
            });
        }

        public class Outlet
        {
            [JsonProperty("id")]
            public string Id { get; private set; }

            [JsonProperty("state")]
            public int State { get; private set; }

            [JsonProperty("alias")]
            public string Alias { get; private set; }

            [JsonProperty("on_time")]
            public int OnTime { get; private set; }
        }
    }
}