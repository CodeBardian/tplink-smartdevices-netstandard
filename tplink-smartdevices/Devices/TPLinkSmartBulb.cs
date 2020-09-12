using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public partial class TPLinkSmartBulb : TPLinkSmartDevice
    {
        private bool _poweredOn;
        private BulbHSV _hsv;
        private int _colorTemp;
        private int _brightness;

        public int Saturation { get; set; }
        public bool IsColor { get; private set; }
        public bool IsDimmable { get; private set; }
        public bool IsVariableColorTemperature { get; private set; }

        /// <summary>
        /// If the bulb is powered on or not
        /// </summary>
        public bool PoweredOn { get; private set; }

        /// <summary>
        /// Bulb color defined by HSV and color temp
        /// </summary>
        public BulbHSV HSV => !IsColor ? throw new NotSupportedException("Bulb does not support color changes.") : _hsv;

        /// <summary>
        /// Color temperature in Kelvin
        /// </summary>
        public int ColorTemperature => !IsVariableColorTemperature
                    ? throw new NotSupportedException("Bulb does not support color temperature changes.")
                    : _colorTemp;

        /// <summary>
        /// Bulb brightness in percent
        /// </summary>
        public int Brightness => !IsDimmable ? throw new NotSupportedException("Bulb does not support dimming.") : _brightness;

        public TPLinkSmartBulb(string hostName, int port = 9999) : base(hostName, port)
        {
            Task.Run(async () => await Refresh()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await ExecuteAsync("system", "get_sysinfo");//
            IsColor = (bool)sysInfo.is_color;
            IsDimmable = (bool)sysInfo.is_dimmable;
            IsVariableColorTemperature = (bool)sysInfo.is_variable_color_temp;
            Saturation = (int)sysInfo.light_state.saturation;

            // todo: retrive the information in lightstate from sysinfo instead (tested in model kl)
            dynamic lightState = await ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "get_light_state"); //
            //_poweredOn = lightState.on_off;
            _poweredOn = sysInfo.light_state.on_off;

            if (!_poweredOn)
            {
                lightState = lightState.dft_on_state;
            }

            // todo: donn't *255 /100 when model is kl
            _hsv = new BulbHSV() { Hue = lightState.hue, Saturation = lightState.saturation, Value = lightState.brightness * 255 / 100 };
            _colorTemp = lightState.color_temp;
            _brightness = lightState.brightness;

            await Refresh(sysInfo);
        }

        /// <summary>
        /// Set Bulb brightness in percent
        /// </summary>
        public Task SetBrightnessAsync(int brightness)
        {
            if (IsDimmable)
            {
                _brightness = brightness;
                return ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "brightness", brightness);
            }
            else
            {
                throw new NotSupportedException("Bulb does not support dimming.");
            }
        }

        /// <summary>
        /// Set Color Temp in Kelvin
        /// </summary>
        public Task SetColorTempAsync(int colortemp)
        {
            if (IsVariableColorTemperature)
            {
                _colorTemp = colortemp;
                return ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "color_temp", colortemp);
            }
            else
            {
                throw new NotSupportedException("Bulb does not support color temperature changes.");
            }
        }

        /// <summary>
        /// Set HSV color
        /// </summary>
        public async Task SetHSVAsync(BulbHSV hsv)
        {
            if (!IsColor)
            {
                throw new NotSupportedException("Bulb does not support color changes.");
            }

            bool isKlOrLbModel = Model.StartsWith("kl", StringComparison.OrdinalIgnoreCase) || Model.StartsWith("lb", StringComparison.OrdinalIgnoreCase);

            // validate arguments
            if (hsv.Hue < 0)
            {
                throw new InvalidOperationException("hue cannot be < 0 or > 100");
            }

            if (hsv.Saturation > 100 || hsv.Saturation < 0)
            {
                throw new InvalidOperationException("saturation cannot be < 0 or > 100");
            }

            const string system = "smartlife.iot.smartbulb.lightingservice";
            const string command = "transition_light_state";

            // tp-link kl model doesn't support sending entire json object
            if (isKlOrLbModel)
            {
                if (hsv.Hue > 360)
                {
                    throw new InvalidOperationException(nameof(hsv.Hue));
                }

                // the mode is always set to normal when allowing color changing
                await ExecuteAsync(system, command, "mode", "normal").ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);

                //await ExecuteAsync(system, command, "color_temp", 0).ConfigureAwait(false);
                //await Task.Delay(100).ConfigureAwait(false);

                if (Brightness != hsv.Brightness)
                {
                    await ExecuteAsync(system, command, "brightness", hsv.Brightness).ConfigureAwait(false);
                    await Task.Delay(100).ConfigureAwait(false);
                }

                if (Saturation != hsv.Saturation)
                {
                    await ExecuteAsync(system, command, "saturation", hsv.Saturation).ConfigureAwait(false);
                    await Task.Delay(100).ConfigureAwait(false);
                }

                if (_hsv.Hue != hsv.Hue)
                {
                    await ExecuteAsync(system, command, "hue", hsv.Hue).ConfigureAwait(false);
                }
            }
            else
            {
                if (hsv.Hue > 100)
                {
                    throw new InvalidOperationException(nameof(hsv.Hue));
                }

                await ExecuteAsync(system, command, "light_state", new JObject
                {
                    new JProperty("hue", hsv.Hue),
                    new JProperty("saturation", hsv.Saturation),
                    new JProperty("brightness", hsv.Value * 100 / 255),
                    new JProperty("color_temp", 0)
                }).ConfigureAwait(false);
            }

            _hsv = hsv;
        }

        /// <summary>
        /// Set power state of bulb
        /// </summary>
        public async Task SetPoweredOn(bool value)
        {
            await ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "on_off", value ? 1 : 0);
            _poweredOn = value;
        }
    }
}
