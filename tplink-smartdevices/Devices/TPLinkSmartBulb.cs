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
        public BulbHSV HSV
        {
            get
            {
                if (!IsColor)
                    throw new NotSupportedException("Bulb does not support color changes.");
                return _hsv;
            }
            private set { }
        }

        /// <summary>
        /// Color temperature in Kelvin
        /// </summary>
        public int ColorTemperature
        {
            get
            {
                if (!IsVariableColorTemperature)
                    throw new NotSupportedException("Bulb does not support color temperature changes.");
                return _colorTemp;
            }
            private set { }
        }

        /// <summary>
        /// Bulb brightness in percent
        /// </summary>
        public int Brightness
        {
            get
            {
                if (!IsDimmable)
                    throw new NotSupportedException("Bulb does not support dimming.");
                return _brightness;
            }
            private set { }
        }

        public TPLinkSmartBulb(string hostName, int port=9999) : base(hostName,port)
        {
            Task.Run(() => Refresh()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo");//
            IsColor = (bool)sysInfo.is_color;
            IsDimmable = (bool)sysInfo.is_dimmable;
            IsVariableColorTemperature = (bool)sysInfo.is_variable_color_temp;

            dynamic lightState = await Execute("smartlife.iot.smartbulb.lightingservice", "get_light_state"); //
            _poweredOn = lightState.on_off;

            if (!_poweredOn)
                lightState = lightState.dft_on_state;
            
            _hsv = new BulbHSV() { Hue = lightState.hue, Saturation = lightState.saturation, Value = lightState.brightness * 255 / 100 };
            _colorTemp = lightState.color_temp;
            _brightness = lightState.brightness;
            
            await Refresh(sysInfo);
        }

        /// <summary>
        /// Set Bulb brightness in percent
        /// </summary>
        public void SetBrightness(int brightness)
        {
            if (IsDimmable)
            {
                Task.Run(async() =>
                {
                    await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "brightness", brightness);
                    _brightness = brightness;
                });
            }
            else
            {
                throw new NotSupportedException("Bulb does not support dimming.");
            }
        }

        /// <summary>
        /// Set Color Temp in Kelvin
        /// </summary>
        public void SetColorTemp(int colortemp)
        {
            if (IsVariableColorTemperature)
            {
                Task.Run(async () =>
                {
                    await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "color_temp", colortemp);
                    _colorTemp = colortemp;
                });
            }
            else
            {
                throw new NotSupportedException("Bulb does not support color temperature changes.");
            }
        }

        /// <summary>
        /// Set HSV color
        /// </summary>
        public void SetHSV(BulbHSV hsv)
        {
            if (IsColor)
            {
                Task.Run(async () =>
                {
                    await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "light_state", new JObject
                    {
                        new JProperty("hue", hsv.Hue),
                        new JProperty("saturation", hsv.Saturation),
                        new JProperty("brightness", (int)(hsv.Value * 100 / 255)),
                        new JProperty("color_temp", 0)
                    });
                    _hsv = hsv;
                });
            }
            else
            {
                throw new NotSupportedException("Bulb does not support color changes.");
            }
        }

        /// <summary>
        /// Set power state of bulb
        /// </summary>
        public void SetPoweredOn(bool value)
        {
            Task.Run(async () =>
            {
                await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "on_off", value ? 1 : 0);
                _poweredOn = value;
            });
        }
    }
}
