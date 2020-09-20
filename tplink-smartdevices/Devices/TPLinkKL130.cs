using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkKL130 : TPLinkSmartBulb
    {
        public int Saturation { get; protected set; }

        public override int ColorTemperature => _colorTemp;
        public string ActiveMode { get; private set; }
        public string Mode { get; protected set; }
        public bool IsFactory { get; protected set; }
        public string Description { get; protected set; }
        public bool IsVariableColorTemp { get; set; }

        public IList<IPreferedLIghtState> PreferedLightStates { get; protected set; }

        public ILightState LightState { get; set; }

        private TPLinkKL130(string hostname) : base(hostname)
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
            _poweredOn = Convert.ToBoolean(sysInfo.GetProperty("light_state").GetProperty("on_off").GetInt32());
            _brightness = sysInfo.GetProperty("light_state").GetProperty("brightness").GetInt32();
            Saturation = sysInfo.GetProperty("light_state").GetProperty("saturation").GetInt32();
            _colorTemp = sysInfo.GetProperty("light_state").GetProperty("color_temp").GetInt32();
            Mode = sysInfo.GetProperty("light_state").GetProperty("mode").GetString();
            Description = sysInfo.GetProperty("description").GetString();
            ActiveMode = sysInfo.GetProperty("active_mode").GetString();
            IsFactory = sysInfo.GetProperty("is_factory").GetBoolean();

            // TODO: does this object change when the light is off? will we have this prop "dft_on_state"?
            /*{{
              "on_off": 0,
              "dft_on_state": {
                "mode": "normal",
                "hue": 270,
                "saturation": 100,
                "color_temp": 5750,
                "brightness": 100
              },
              "err_code": 0
            }}*/

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

        public override Task SetHSVAsync(BulbHSV hsv)
        {
            return base.SetHSVAsync(hsv);
        }

        // TODO!
        public static async Task<TPLinkKL130> CreateNew(string hostname)
        {
            // get data from the buld
            // init new buld instance
            var smartBulb = new TPLinkKL130(hostname);
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
        public bool OnOff => throw new NotImplementedException();

        public int Hue => throw new NotImplementedException();

        public int Saturation => throw new NotImplementedException();

        public int ColorTemp => throw new NotImplementedException();

        public int Brightness => throw new NotImplementedException();
    }

    public class PreferedLightState : IPreferedLIghtState
    {
        public int Index => throw new NotImplementedException();

        public int Hue => throw new NotImplementedException();

        public int Saturation => throw new NotImplementedException();

        public int ColorTemp => throw new NotImplementedException();

        public int Brightness => throw new NotImplementedException();
    }
}
