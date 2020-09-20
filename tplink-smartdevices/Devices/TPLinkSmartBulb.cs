using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public partial class TPLinkSmartBulb : TPLinkSmartDevice
    {
        protected bool _poweredOn;
        protected BulbHSV _hsv;
        protected int _colorTemp;
        protected int _brightness;

        public bool IsColor { get; protected set; }
        public bool IsDimmable { get; protected set; }
        public bool IsVariableColorTemperature { get; protected set; }

        /// <summary>
        /// If the bulb is powered on or not
        /// </summary>
        public bool PoweredOn { get; protected set; }

        /// <summary>
        /// Bulb color defined by HSV and color temp
        /// </summary>
        public BulbHSV HSV => !IsColor ? throw new NotSupportedException("Bulb does not support color changes.") : _hsv;

        /// <summary>
        /// Color temperature in Kelvin
        /// </summary>
        public virtual int ColorTemperature => !IsVariableColorTemperature
                    ? throw new NotSupportedException("Bulb does not support color temperature changes.")
                    : _colorTemp;

        /// <summary>
        /// Bulb brightness in percent
        /// </summary>
        public int Brightness => !IsDimmable ? throw new NotSupportedException("Bulb does not support dimming.") : _brightness;

        public TPLinkSmartBulb(string hostName, int port = 9999) : base(hostName, port)
        {
            //Task.Run(async () => await RefreshAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public virtual async Task RefreshAsync()
        {
            //var sysInfo = await ExecuteAsync("system", "get_sysinfo").ConfigureAwait(false);
            //IsColor = (bool)sysInfo.is_color;
            //IsDimmable = (bool)sysInfo.is_dimmable;
            //IsVariableColorTemperature = (bool)sysInfo.is_variable_color_temp;

            //try
            //{
            //    // not supported by all smart bulb - so that exception is very likely to happen
            //    Saturation = (int)sysInfo.light_state.saturation;
            //}
            //catch
            //{
            //    Saturation = 0;
            //}

            //// todo: retrive the information in lightstate from sysinfo instead (tested in model kl)
            //var lightState = await ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "get_light_state").ConfigureAwait(false);

            ////_poweredOn = lightState.on_off;
            //_poweredOn = sysInfo.light_state.on_off;


            ///*{{
            //  "on_off": 0,
            //  "dft_on_state": {
            //    "mode": "normal",
            //    "hue": 270,
            //    "saturation": 100,
            //    "color_temp": 5750,
            //    "brightness": 100
            //  },
            //  "err_code": 0
            //}}*/

            //try
            //{
            //    _colorTemp = lightState.color_temp;
            //}
            //catch
            //{

            //}

            //_brightness = lightState.brightness;

            //if (!_poweredOn)
            //{
            //    lightState = lightState.dft_on_state;
            //}

            //// TODO: ADD configureaAwait();
            ////await RefreshAsync(sysInfo)
            ////await RefreshAsync().ConfigureAwait(false);
            //await RefreshAsync(sysInfo).ConfigureAwait(false);

            //// todo: don't use the factor of 255 / 100 for kl/lb models
            //if (Model != null && (Model.StartsWith("kl", StringComparison.OrdinalIgnoreCase) || Model.StartsWith("lb", StringComparison.OrdinalIgnoreCase)))
            //{
            //    _hsv = new BulbHSV()
            //    {
            //        Hue = lightState.hue,
            //        Saturation = lightState.saturation,
            //        Value = lightState.brightness <= 100 ? lightState.brightness : lightState.brightness * 100 / 255
            //    };
            //}
            //else
            //{
            //    _hsv = new BulbHSV()
            //    {
            //        Hue = lightState.hue,
            //        Saturation = lightState.saturation,
            //        Value = lightState.brightness * 255 / 100
            //    };
            //}
        }

        /// <summary>
        /// Set Bulb brightness in percent
        /// </summary>
        public virtual Task SetBrightnessAsync(int brightness)
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
        public virtual Task SetColorTempAsync(int colortemp)
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
        public virtual async Task SetHSVAsync(BulbHSV hsv)
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

                // TODO: INCLUDE
                //if (Saturation != hsv.Saturation)
                //{
                //    await ExecuteAsync(system, command, "saturation", hsv.Saturation).ConfigureAwait(false);
                //    await Task.Delay(100).ConfigureAwait(false);
                //}

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

                // TODO: INCLUDE
                //await ExecuteAsync(system, command, "light_state", new JObject
                //{
                //    new JProperty("hue", hsv.Hue),
                //    new JProperty("saturation", hsv.Saturation),
                //    new JProperty("brightness", hsv.Value * 100 / 255),
                //    new JProperty("color_temp", 0)
                //}).ConfigureAwait(false);
            }

            _hsv = hsv;
        }

        /// <summary>
        /// Set power state of bulb
        /// </summary>
        public virtual async Task SetPoweredOn(bool value)
        {
            await ExecuteAsync("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "on_off", value ? 1 : 0);
            _poweredOn = value;
        }

    }
}
