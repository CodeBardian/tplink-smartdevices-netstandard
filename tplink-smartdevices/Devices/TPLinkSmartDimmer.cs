using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Devices
{
    public partial class TPLinkSmartDimmer : TPLinkSmartPlug
    {
        private DimmerOptions _options;
        private bool _poweredOn;
        private int _brightness;
        private int[] _presets;

        public bool PoweredOn => _poweredOn;
        public int Brightness => _brightness;
        public int[] Presets => _presets;
        public DimmerOptions Options => _options;

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

        public new async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);
            _poweredOn = (int)sysInfo.relay_state == 1;
            _brightness = (int)sysInfo.brightness;

            dynamic defaultBehavior = await Execute("smartlife.iot.dimmer", "get_default_behavior").ConfigureAwait(false);
            string long_press = (string)defaultBehavior.long_press.mode;
            string double_click = (string)defaultBehavior.double_click.mode;
            _options.LongPressAction = long_press.ToDimmerMode();
            _options.DoubleClickAction = double_click.ToDimmerMode();

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
            if (brightness < 0 || brightness > 100) throw new ArgumentException("brightness should be between 0 and 100");

            await Execute("smartlife.iot.dimmer", "set_dimmer_transition", new JObject
            {
                new JProperty("brightness", brightness),
                new JProperty("mode", (mode ?? _options.Mode).ToStr()),
                new JProperty("duration", duration ?? 1),
            }).ConfigureAwait(false);
            _brightness = brightness;
        }

        /// <summary>
        /// Instantly change plug to a specified brightness level
        /// </summary>
        public async Task SetBrightness(int brightness)
        {
            if (brightness < 0 || brightness > 100) throw new ArgumentException("brightness should be between 0 and 100");

            await Execute("smartlife.iot.dimmer", "set_brightness", "brightness", brightness).ConfigureAwait(false);
            _brightness = brightness;
        }

        /// <summary>
        /// Configures change mode on double click of switch
        /// </summary>
        public async Task SetDoubleClickAction(DimmerMode mode, int index=0)
        {
            if (mode == DimmerMode.Preset)
            {
                if (index < 0 || index > 3) throw new ArgumentException("index should be between 0 and 3");

                await Execute("smartlife.iot.dimmer", "set_double_click_action", new JObject
                {
                    new JProperty("mode", mode.ToStr()),
                    new JProperty("index", index)
                }).ConfigureAwait(false);
            }
            else 
                await Execute("smartlife.iot.dimmer", "set_double_click_action", "mode", mode.ToStr()).ConfigureAwait(false);

            _options.DoubleClickAction = mode;
        }

        /// <summary>
        /// Configures change mode on long press of switch
        /// </summary>
        public async Task SetLongPressAction(DimmerMode mode, int index=0)
        {
            if (mode == DimmerMode.Preset)
            {
                if (index < 0 || index > 3) throw new ArgumentException("index should be between 0 and 3");

                await Execute("smartlife.iot.dimmer", "set_long_press_action", new JObject
                { 
                    new JProperty("mode", mode.ToStr()),
                    new JProperty("index", index)
                }).ConfigureAwait(false);
            }
            else 
                await Execute("smartlife.iot.dimmer", "set_long_press_action", "mode", mode.ToStr()).ConfigureAwait(false);

            _options.LongPressAction = mode;
        }

        /// <summary>
        /// Configures speed of fade on transition
        /// </summary>
        public async Task SetFadeOnTime(int fadeOnTime)
        {
            if (fadeOnTime < 0) throw new ArgumentException("fadeOnTime should be a positive number");

            await Execute("smartlife.iot.dimmer", "set_fade_on_time", "fadeTime", fadeOnTime).ConfigureAwait(false);
            _options.FadeOnTime = fadeOnTime;
        }

        /// <summary>
        /// Configures speed of fade off transition
        /// </summary>
        public async Task SetFadeOffTime(int fadeOffTime)
        {
            if (fadeOffTime < 0) throw new ArgumentException("fadeOffTime should be a positive number");

            await Execute("smartlife.iot.dimmer", "set_fade_on_time", "fadeTime", fadeOffTime).ConfigureAwait(false);
            _options.FadeOffTime = fadeOffTime;
        }
    }
}
