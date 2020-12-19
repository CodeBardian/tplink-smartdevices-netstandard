using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Devices
{
    public partial class TPLinkSmartDimmer : TPLinkSmartDevice
    {
        private DimmerOptions _options;
        private bool _poweredOn;
        private int _brightness;
        private int[] _presets;

        public bool PoweredOn => _poweredOn;

        [Obsolete("Use async factory method TPLinkSmartDimmer.Create() instead")]
        public TPLinkSmartDimmer(string hostName, int port = 9999, DimmerOptions opts = null) : base(hostName, port)
        {
            Task.Run(async () =>
            {
                this._options = opts ?? new DimmerOptions();
                await Refresh().ConfigureAwait(false);
            }).GetAwaiter().GetResult();
        }

        private TPLinkSmartDimmer() { }

        public static async Task<TPLinkSmartDimmer> Create(string hostname, int port = 9999, DimmerOptions opts = null)
        {
            var d = new TPLinkSmartDimmer() { Hostname = hostname, Port = port };
            d._options = opts ?? new DimmerOptions();
            await d.Refresh().ConfigureAwait(false);
            return d;
        }

        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);
            _poweredOn = (int)sysInfo.relay_state == 1;
            _brightness = (int)sysInfo.brightness;

            RetrievePresets(sysInfo);
            await Refresh((object)sysInfo).ConfigureAwait(false);
        }

        private void RetrievePresets(dynamic sysinfo)
        {
            JArray presets = JArray.Parse(Convert.ToString(sysinfo.preferred_state));
            _presets = presets.Select(x => (int)x["brightness"]).ToArray();
        }

        /// <summary>
        /// Set power state of dimming switch
        /// </summary>
        public async Task SetPoweredOn(bool value)
        {
            await Execute("smartlife.iot.dimmer", "set_switch_state", "state", value ? 1 : 0).ConfigureAwait(false);
            _poweredOn = value;
        }

        /// <summary>
        /// Transition to a specified brightness level
        /// </summary>
        public async Task TransitionBrightness(int brightness, DimmerMode? mode = null, int? duration = null)
        {
            await Execute("smartlife.iot.dimmer", "set_dimmer_transition", new JObject
            {
                new JProperty("brightness", brightness),
                new JProperty("mode", mode ?? _options.Mode),
                new JProperty("duration", duration ?? 1),
            }).ConfigureAwait(false);
            _brightness = brightness;
        }

        /// <summary>
        /// Instantly change plug to a specified brightness level
        /// </summary>
        public async Task SetBrightness(int brightness)
        {
            await Execute("smartlife.iot.dimmer", "set_brightness", "brightness", brightness).ConfigureAwait(false);
            _brightness = brightness;
        }

        /// <summary>
        /// Configures change mode on double click of switch
        /// </summary>
        public async Task SetDoubleClickAction(DimmerMode mode)
        {
            await Execute("smartlife.iot.dimmer", "set_long_press_action", "mode", mode).ConfigureAwait(false);
        }
    }
}
