using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPLinkSmartDevices.Devices;

namespace TPLinkSmartDevices.Data.CountDownRule
{
    public class CountDownRule
    {
        /// <summary>
        /// identifier of CountDown rule
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// custom name of CountDown rule
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// if the rule is currently active or not
        /// </summary>
        [JsonProperty("enable")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Enabled { get; set; }

        /// <summary>
        /// if the device should be powered on or off after the timer runs out
        /// </summary>
        [JsonProperty("act")]
        [JsonConverter(typeof(BoolConverter))]
        public bool PoweredOn { get; set; }

        /// <summary>
        /// delay in seconds after which the action triggers 
        /// </summary>
        [JsonProperty("delay")]
        public int Delay { get; set; }

        public bool ShouldSerializeId()
        {
            return Id != null;
        }
    }

    internal static class CountDownExtensions
    {
        internal static async Task<List<CountDownRule>> RetrieveCountDownRules(this ICountDown device, string ns)
        {
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "get_rules").ConfigureAwait(false);
            string rule_list = Convert.ToString(result.rule_list);
            return JsonConvert.DeserializeObject<List<CountDownRule>>(rule_list);
        }

        internal static async Task<CountDownRule> AddCountDownRule(this ICountDown device, string ns, CountDownRule cdr)
        {
            JObject payload = JObject.FromObject(cdr);
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "add_rule", payload).ConfigureAwait(false);
            cdr.Id = (string)result.id;
            return cdr;
        }

        internal static async Task EditCountDownRule(this ICountDown device, string ns, CountDownRule cdr)
        {
            JObject payload = JObject.FromObject(cdr);
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "edit_rule", payload).ConfigureAwait(false);
        }
    }
}
