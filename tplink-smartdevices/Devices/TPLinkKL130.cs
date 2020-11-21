using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkKL130 : TPLinkSmartBulb
    {
        public int Saturation { get; protected set; }
        public override int ColorTemperature => _colorTemp;
        public string ActiveMode { get; protected set; }
        public string Mode { get; protected set; }
        public bool IsFactory { get; protected set; }
        public string Description { get; protected set; }
        public bool IsVariableColorTemp { get; protected set; }

        public IList<IPreferedLIghtState> PreferedLightStates { get; protected set; }

        public ILightState LightState { get; set; }

        private TPLinkKL130(string ipAddress) : base(ipAddress)
        {
        }

        public async override Task RefreshAsync()
        {
            //return base.RefreshAsync();
            JsonElement sysInfo = await ExecuteAsync("system", "get_sysinfo").ConfigureAwait(false);
            IsColor = Convert.ToBoolean(sysInfo.GetProperty("is_color").GetInt32());
            SoftwareVersion = sysInfo.GetProperty("sw_ver").GetString();
            Alias = sysInfo.GetProperty("alias").GetString();
            Model = sysInfo.GetProperty("model").GetString();
            MacAddress = sysInfo.GetProperty("mic_mac").GetString();
            HardwareId = sysInfo.GetProperty("hwId").GetString();
            HardwareVersion = sysInfo.GetProperty("hw_ver").GetString();
            OemId = sysInfo.GetProperty("oemId").GetString();
            RSSI = sysInfo.GetProperty("rssi").GetInt32();
            DeviceId = sysInfo.GetProperty("deviceId").GetString();
            IsDimmable = Convert.ToBoolean(sysInfo.GetProperty("is_dimmable").GetInt32());
            IsVariableColorTemp = Convert.ToBoolean(sysInfo.GetProperty("is_variable_color_temp").GetInt32());
            IsVariableColorTemperature = Convert.ToBoolean(sysInfo.GetProperty("is_variable_color_temp").GetInt32());

            JsonElement lightState = sysInfo.GetProperty("light_state");
            _poweredOn = Convert.ToBoolean(lightState.GetProperty("on_off").GetInt32());

            // when on off state the object changes
            if (!_poweredOn)
            {
                lightState = lightState.GetProperty("dft_on_state");
            }

            // load light state
            Saturation = lightState.GetProperty("saturation").GetInt32();
            _brightness = lightState.GetProperty("brightness").GetInt32();
            _colorTemp = lightState.GetProperty("color_temp").GetInt32();
            Mode = lightState.GetProperty("mode").GetString();
            Description = sysInfo.GetProperty("description").GetString();
            ActiveMode = sysInfo.GetProperty("active_mode").GetString();
            IsFactory = sysInfo.GetProperty("is_factory").GetBoolean();

            // load prefered light states
            PreferedLightStates = sysInfo.GetProperty("preferred_state").EnumerateArray().Select(obj => new PreferedLightState
            {
                Index = obj.GetProperty("index").GetInt32(),
                Hue = obj.GetProperty("hue").GetInt32(),
                Saturation = obj.GetProperty("saturation").GetInt32(),
                ColorTemp = obj.GetProperty("color_temp").GetInt32(),
                Brightness = obj.GetProperty("brightness").GetInt32(),
            }).ToList<IPreferedLIghtState>();

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

        public async override Task SetHSVAsync(BulbHSV hsv)
        {
            // validate hsv model
            ValidateHsv(hsv);

            // validate heu (it's represented in degrees)
            if (hsv.Hue > 360)
            {
                throw new InvalidOperationException(nameof(hsv.Hue));
            }

            const string system = "smartlife.iot.smartbulb.lightingservice";
            const string command = "transition_light_state";

            // the mode is always set to normal when allowing color changing
            await ExecuteAsync(system, command, "mode", "normal").ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await ExecuteAsync(system, command, "color_temp", 0).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
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

        public static async Task<TPLinkKL130> CreateNew(string ipAddress)
        {
            var smartBulb = new TPLinkKL130(ipAddress);
            await smartBulb.RefreshAsync().ConfigureAwait(false);
            return smartBulb;
        }
    }

    public interface IKL130LightState
    {
        int Hue { get; }
        int Saturation { get; }
        int ColorTemp { get; }
        int Brightness { get; }
    }

    public interface ILightState : IKL130LightState
    {
        bool OnOff { get; }
    }

    public interface IPreferedLIghtState : IKL130LightState
    {
        int Index { get; }
    }

    public class LightState : ILightState
    {
        public int Hue { get; set; }
        public int Saturation { get; set; }
        public int ColorTemp { get; set; }
        public int Brightness { get; set; }
        public bool OnOff { get; set; }
    }

    public class PreferedLightState : IPreferedLIghtState
    {
        public int Index { get; set; }
        public int Hue { get; set; }
        public int Saturation { get; set; }
        public int ColorTemp { get; set; }
        public int Brightness { get; set; }
    }
}
