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
        public string Id { get; set; }

        /// <summary>
        /// custom name of CountDown rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// if the rule is currently active or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// if the device should be powered on or off after the timer runs out
        /// </summary>
        public bool PoweredOn { get; set; }

        /// <summary>
        /// delay in seconds after which the action triggers 
        /// </summary>
        public int Delay { get; set; }
    }

    public static class CountDownExtensions
    {
        internal static async Task<List<CountDownRule>> RetrieveCountDownRules(this ICountDown device, string ns)
        {
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "get_rules").ConfigureAwait(false);
            JArray ruleList = JArray.Parse(Convert.ToString(result.rule_list));
            return ruleList.Count != 0 ? ruleList.Select(x => new CountDownRule
            {
                Id = (string)x["id"],
                Enabled = (bool)x["enable"],
                Delay = (int)x["delay"],
                PoweredOn = (bool)x["act"],
                Name = (string)x["name"],
            }).ToList() : new List<CountDownRule>();
        }

        internal static async Task<CountDownRule> AddCountDownRule(this ICountDown device, string ns, CountDownRule cdr)
        {
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "add_rule", new JObject
            {
                new JProperty("enable", cdr.Enabled),
                new JProperty("delay", cdr.Delay),
                new JProperty("act", cdr.PoweredOn),
                new JProperty("name", cdr.Name ?? "countdown rule")
            }, null).ConfigureAwait(false);
            cdr.Id = (string)result.id;
            return cdr;
        }

        internal static async Task EditCountDownRule(this ICountDown device, string ns, CountDownRule cdr)
        {
            dynamic result = await ((TPLinkSmartDevice)device).Execute(ns, "edit_rule", new JObject
            {
                new JProperty("id", cdr.Id),
                new JProperty("enable", cdr.Enabled),
                new JProperty("delay", cdr.Delay),
                new JProperty("act", cdr.PoweredOn),
                new JProperty("name", cdr.Name)
            }, null).ConfigureAwait(false);
        }
    }
}
