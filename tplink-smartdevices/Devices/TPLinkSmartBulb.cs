﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<PreferredLightState> _preferredLightStates;

        public LightDetails LightDetails { get; private set; }

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

        private List<PreferredLightState> PreferredLightStates => _preferredLightStates;

        public TPLinkSmartBulb(string hostName, int port=9999) : base(hostName,port)
        {
            Task.Run(async() => await Refresh()).GetAwaiter().GetResult();
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
            _poweredOn = (bool)lightState.on_off;

            if (!_poweredOn)
                lightState = lightState.dft_on_state;
            
            _hsv = new BulbHSV() { Hue = (int)lightState.hue, Saturation = (int)lightState.saturation, Value = (int)lightState.brightness };
            _colorTemp = (int)lightState.color_temp;
            _brightness = (int)lightState.brightness;

            dynamic lightDetails = await Execute("smartlife.iot.smartbulb.lightingservice", "get_light_details");
            LightDetails = JsonConvert.DeserializeObject<LightDetails>(Convert.ToString(lightDetails));

            await RetrievePresets();

            await Refresh(sysInfo);
        }

        public void GetLightDetails()
        {
            Task.Run(async () =>
            {
                dynamic lightDetails = await Execute("smartlife.iot.smartbulb.lightingservice", "get_light_details");
                var test = lightDetails.test;
            });
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
                    dynamic result = await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", new JObject
                    {
                        new JProperty("hue", hsv.Hue),
                        new JProperty("saturation", hsv.Saturation),
                        new JProperty("brightness", hsv.Value),
                        new JProperty("color_temp", 0)
                    }, null);
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

        /// <summary>
        /// Operate bulb on one of four presets
        /// </summary>
        /// <param name = "presetIndex" >Index of the four presets, ranging from 0 to 3</param>
        public void ApplyPreset(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex > 3) throw new ArgumentOutOfRangeException("preset index needs to be between 0 and 3");

            if (PreferredLightStates.Count == 0) throw new Exception("no light state presets found");

            PreferredLightState preset = PreferredLightStates[presetIndex];
            if (preset.ColorTemperature != 0)
            {
                SetColorTemp(preset.ColorTemperature);
                SetBrightness(preset.HSV.Value);
            }
            else
            {
                SetHSV(preset.HSV);
            }
        }

        async Task RetrievePresets()
        {
            dynamic result = await Execute("smartlife.iot.smartbulb.lightingservice", "get_preferred_state");
            JArray presets = JArray.Parse(Convert.ToString(result.states));
            _preferredLightStates = presets.Select(x => new PreferredLightState
            {
                ColorTemperature = (int)x["color_temp"],
                HSV = new BulbHSV() { Hue = (int)x["hue"], Saturation = (int)x["saturation"], Value = (int)x["brightness"] }
            }).ToList();
        }


        private class PreferredLightState
        {
            public BulbHSV HSV { get; set; }
            public int ColorTemperature { get; set; }
        }
    }
}
