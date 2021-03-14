using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;
using TPLinkSmartDevices.Data.CountDownRule;
using TPLinkSmartDevices.Data.Schedule;

namespace TPLinkSmartDevices.Devices
{
    public partial class TPLinkSmartBulb : TPLinkSmartDevice, ICountDown, ISchedule
    {
        private const string COUNTDOWN_NAMESPACE = "smartlife.iot.common.count_down";
        private const string SCHEDULE_NAMESPACE = "smartlife.iot.common.schedule";

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
        public bool PoweredOn => _poweredOn;

        /// <summary>
        /// Bulb color defined by HSV and color temp
        /// </summary>
        public BulbHSV HSV => IsColor ? _hsv : throw new NotSupportedException("Bulb does not support color changes.");

        /// <summary>
        /// Color temperature in Kelvin
        /// </summary>
        public int ColorTemperature => IsVariableColorTemperature ? _colorTemp : throw new NotSupportedException("Bulb does not support color temperature changes.");

        /// <summary>
        /// Bulb brightness in percent
        /// </summary>
        public int Brightness => IsDimmable ? _brightness : throw new NotSupportedException("Bulb does not support dimming.");

        public List<PreferredLightState> PreferredLightStates => _preferredLightStates;
        public List<CountDownRule> CountDownRules { get; private set; }
        public List<Schedule> Schedules { get; private set; }

        public TPLinkSmartBulb(string hostName, int port=9999) : base(hostName,port)
        {
            Task.Run(async() => await Refresh()).GetAwaiter().GetResult();
        }

        private TPLinkSmartBulb() { }

        public static async Task<TPLinkSmartBulb> Create(string hostname, int port = 9999)
        {
            var b = new TPLinkSmartBulb() { Hostname = hostname, Port = port };
            await b.Refresh().ConfigureAwait(false);
            return b;
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);//
            IsColor = (bool)sysInfo.is_color;
            IsDimmable = (bool)sysInfo.is_dimmable;
            IsVariableColorTemperature = (bool)sysInfo.is_variable_color_temp;

            dynamic lightState = await Execute("smartlife.iot.smartbulb.lightingservice", "get_light_state").ConfigureAwait(false); //
            _poweredOn = (bool)lightState.on_off;

            if (!_poweredOn)
                lightState = lightState.dft_on_state;
            
            _hsv = new BulbHSV() { Hue = (int)lightState.hue, Saturation = (int)lightState.saturation, Value = (int)lightState.brightness };
            _colorTemp = (int)lightState.color_temp;
            _brightness = (int)lightState.brightness;
            
            dynamic lightDetails = await Execute("smartlife.iot.smartbulb.lightingservice", "get_light_details").ConfigureAwait(false);
            LightDetails = JsonConvert.DeserializeObject<LightDetails>(Convert.ToString(lightDetails));

            await RetrievePresets().ConfigureAwait(false);
            await RetrieveCountDownRules().ConfigureAwait(false);
            await RetrieveSchedules().ConfigureAwait(false);
            await Refresh((object)sysInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Set Bulb brightness in percent
        /// </summary>
        public async Task SetBrightness(int brightness, int transition_period = 0)
        {
            if (transition_period < 0 || transition_period > 10000) throw new ArgumentException("transition_period only allows values between 0 and 10000");

            if (!IsDimmable) throw new NotSupportedException("Bulb does not support dimming.");

            await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", new JObject
            {
                new JProperty("brightness", brightness),
                new JProperty("transition_period", transition_period)
            }, null).ConfigureAwait(false);
            _brightness = brightness;
        }

        /// <summary>
        /// Set Color Temp in Kelvin
        /// </summary>
        public async Task SetColorTemp(int colortemp, int transition_period = 0)
        {
            if (transition_period < 0 || transition_period > 10000) throw new ArgumentException("transition_period only allows values between 0 and 10000");

            if (!IsVariableColorTemperature) throw new NotSupportedException("Bulb does not support color temperature changes.");

            await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", new JObject
            {
                new JProperty("color_temp", colortemp),
                new JProperty("transition_period", transition_period)
            }, null).ConfigureAwait(false);
            _colorTemp = colortemp;
        }

        /// <summary>
        /// Set HSV color
        /// </summary>
        public async Task SetHSV(BulbHSV hsv, int transition_period = 0)
        {
            if (transition_period < 0 || transition_period > 10000) throw new ArgumentException("transition_period only allows values between 0 and 10000");

            if (!IsColor) throw new NotSupportedException("Bulb does not support color changes.");
            
            dynamic result = await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", new JObject
            {
                new JProperty("hue", hsv.Hue),
                new JProperty("saturation", hsv.Saturation),
                new JProperty("brightness", hsv.Value),
                new JProperty("color_temp", 0),
                new JProperty("transition_period", transition_period)
            }, null).ConfigureAwait(false);
            _hsv = hsv;
        }

        /// <summary>
        /// Set power state of bulb
        /// </summary>
        public override async Task SetPoweredOn(bool value)
        {
            await Execute("smartlife.iot.smartbulb.lightingservice", "transition_light_state", "on_off", value ? 1 : 0).ConfigureAwait(false);
            _poweredOn = value;
        }

        /// <summary>
        /// Operate bulb on one of four presets
        /// </summary>
        /// <param name = "presetIndex" >Index of the four presets, ranging from 0 to 3</param>
        public async Task ApplyPreset(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex > 3) throw new ArgumentOutOfRangeException("preset index needs to be between 0 and 3");

            if (PreferredLightStates.Count == 0) throw new Exception("no light state presets found");

            PreferredLightState preset = PreferredLightStates[presetIndex];
            if (preset.ColorTemperature != 0)
            {
                await SetColorTemp(preset.ColorTemperature).ConfigureAwait(false);
                await SetBrightness(preset.HSV.Value).ConfigureAwait(false);
            }
            else
            {
                await SetHSV(preset.HSV).ConfigureAwait(false);
            }
        }

        async Task RetrievePresets()
        {
            dynamic result = await Execute("smartlife.iot.smartbulb.lightingservice", "get_preferred_state").ConfigureAwait(false);
            JArray presets = JArray.Parse(Convert.ToString(result.states));
            _preferredLightStates = presets.Select(x => new PreferredLightState
            {
                ColorTemperature = (int)x["color_temp"],
                HSV = new BulbHSV() { Hue = (int)x["hue"], Saturation = (int)x["saturation"], Value = (int)x["brightness"] }
            }).ToList();
        }

        public async Task RetrieveCountDownRules()
        {
            CountDownRules = await this.RetrieveCountDownRules(COUNTDOWN_NAMESPACE);
        }

        public async Task AddCountDownRule(CountDownRule cdr)
        {
            if (CountDownRules.Any(c => c.Id == cdr.Id)) throw new Exception("countdown rule with specified id already exists");

            cdr = await this.AddCountDownRule(COUNTDOWN_NAMESPACE, cdr);
            CountDownRules.Add(cdr);
        }

        public async Task EditCountDownRule(string id, bool? enabled = null, int? delay = null, bool? poweredOn = null, string name = null)
        {
            CountDownRule cdr = CountDownRules.Find(c => c.Id == id);

            if (cdr == null) throw new Exception("plug has no countdown rule with specified id");

            cdr.Enabled = enabled ?? cdr.Enabled;
            cdr.Delay = delay ?? cdr.Delay;
            cdr.PoweredOn = poweredOn ?? cdr.PoweredOn;
            cdr.Name = name ?? cdr.Name;

            await this.EditCountDownRule(COUNTDOWN_NAMESPACE, cdr);
        }

        public async Task EditCountDownRule(CountDownRule newCdr)
        {
            if (newCdr.Id == null) throw new Exception("countdown rule id is required");
            if (CountDownRules.Any(c => c.Id == newCdr.Id)) throw new Exception("countdown rule with specified id already exists");

            CountDownRule cdr = CountDownRules.Find(c => c.Id == newCdr.Id);
            cdr.Enabled = newCdr.Enabled;
            cdr.Delay = newCdr.Delay;
            cdr.PoweredOn = newCdr.PoweredOn;
            cdr.Name = newCdr.Name ?? cdr.Name;

            await this.EditCountDownRule(COUNTDOWN_NAMESPACE, cdr);
        }


        public async Task DeleteCountDownRule(string id)
        {
            dynamic result = await Execute(COUNTDOWN_NAMESPACE, "delete_rule", "id", id).ConfigureAwait(false);

            CountDownRules.RemoveAll(c => c.Id == id);
        }

        public async Task DeleteAllCountDownRules()
        {
            dynamic result = await Execute(COUNTDOWN_NAMESPACE, "delete_all_rules").ConfigureAwait(false);

            CountDownRules.Clear();
        }

        public async Task RetrieveSchedules()
        {
            Schedules = await this.RetrieveSchedules(SCHEDULE_NAMESPACE);
        }

        public async Task AddSchedule(Schedule schedule)
        {
            await this.AddSchedule(SCHEDULE_NAMESPACE, schedule);
        }

        public async Task EditSchedule(Schedule schedule)
        {
            await this.EditSchedule(SCHEDULE_NAMESPACE, schedule);
        }

        public async Task DeleteSchedule(string id)
        {
            dynamic result = await Execute(SCHEDULE_NAMESPACE, "delete_rule", "id", id).ConfigureAwait(false);
            Schedules.RemoveAll(c => c.Id == id);
        }

        public async Task DeleteSchedules()
        {
            dynamic result = await Execute(SCHEDULE_NAMESPACE, "delete_all_rules").ConfigureAwait(false);
            Schedules.Clear();
        }

        public class PreferredLightState
        {
            public BulbHSV HSV { get; set; }
            public int ColorTemperature { get; set; }
        }
    }
}
