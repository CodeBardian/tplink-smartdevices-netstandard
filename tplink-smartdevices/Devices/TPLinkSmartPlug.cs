using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data.CountDownRule;
using TPLinkSmartDevices.Data.Schedule;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartPlug : TPLinkSmartDevice, ICountDown, ISchedule
    {
        private const string COUNTDOWN_NAMESPACE = "count_down";
        private const string SCHEDULE_NAMESPACE = "schedule";

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
        public List<CountDownRule> CountDownRules { get; private set; }

        public List<Schedule> Schedules { get; private set; }

        public TPLinkSmartPlug(string hostname, int port = 9999) : base(hostname, port)
        {
            Task.Run(() => Refresh()).GetAwaiter().GetResult();
        }

        protected TPLinkSmartPlug() { }

        public static async Task<TPLinkSmartPlug> Create(string hostname, int port = 9999)
        {
            var p = new TPLinkSmartPlug() { Hostname = hostname, Port = port };
            await p.Refresh().ConfigureAwait(false);
            return p;
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);

            JObject info = JObject.Parse(Convert.ToString(sysInfo));
            bool hasChildren = info["children"] != null;

            if (hasChildren) throw new Exception("this plug has multiple outlets. use TPLinkSmartMultiPlug instead!");

            OutletPowered = (int)sysInfo.relay_state == 1;
            Features = ((string)sysInfo.feature).Split(':');
            LedOn = !(bool)sysInfo.led_off;
            
            if ((int)sysInfo.on_time == 0)
                PoweredOnSince = default(DateTime);
            else
                PoweredOnSince = DateTime.Now - TimeSpan.FromSeconds((int)sysInfo.on_time);

            await RetrieveCountDownRules().ConfigureAwait(false);
            await RetrieveSchedules().ConfigureAwait(false);
            await Refresh((object)sysInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Send command which changes power state to plug
        /// </summary>
        public override async Task SetPoweredOn(bool value)
        {
            await Execute("system", "set_relay_state", "state", value ? 1 : 0).ConfigureAwait(false);
            this.OutletPowered = value;
        }

        /// <summary>
        /// Send command which changes power state to plug
        /// </summary>
        [Obsolete("Use TPLinkSmartPlug.SetPoweredOn(bool) instead")]
        public void SetOutletPowered(bool value)
        {
            Task.Run(async () =>
            {
                await Execute("system", "set_relay_state", "state", value ? 1 : 0).ConfigureAwait(false);
                this.OutletPowered = value;
            });
        }

        private object GetPlugID(int outletId)
        {
            return JArray.FromObject(new [] {$"{DeviceId}0{outletId}"});
        }

        /// <summary>
        /// Send command which enables or disables night mode (LED state)
        /// </summary>
        public async Task SetLedOn(bool value)
        {
            await Execute("system", "set_led_off", "off", value ? 0 : 1).ConfigureAwait(false);
            this.LedOn = value;
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
            if (!CountDownRules.Any(c => c.Id == newCdr.Id)) throw new Exception("plug has no countdown rule with specified id");

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
            dynamic result = await Execute(SCHEDULE_NAMESPACE, "get_rules").ConfigureAwait(false);
            string rule_list = Convert.ToString(result.rule_list);
            Schedules = JsonConvert.DeserializeObject<List<Schedule>>(rule_list);
        }

        public async Task AddSchedule(Schedule schedule)
        {
            try
            {
                string payload = JsonConvert.SerializeObject(schedule);
                dynamic result = await Execute(SCHEDULE_NAMESPACE, "add_rule", payload).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task EditSchedule(Schedule schedule)
        {
            try
            {
                JObject payload = JObject.FromObject(schedule);
                dynamic result = await Execute(SCHEDULE_NAMESPACE, "edit_rule", payload).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e;
            }
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
    }
}