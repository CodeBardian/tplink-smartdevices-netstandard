using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartMeterPlug : TPLinkSmartPlug
    {
        private const double  WATTS_IN_KILOWATT = 1000d;
        private dynamic _gainData;

        public PowerData CurrentPowerUsage { get; private set; }
        public uint VGain => _gainData.vgain;
        public uint IGain => _gainData.igain;

        public TPLinkSmartMeterPlug(string hostname) : base(hostname)
        {
            Task.Run(async() =>
            {
                await Refresh();   
            }).GetAwaiter().GetResult();
        }

        private TPLinkSmartMeterPlug() {  }

        public static new async Task<TPLinkSmartMeterPlug> Create(string hostname, int port = 9999)
        {
            var p = new TPLinkSmartMeterPlug() { Hostname = hostname, Port = port };
            await p.Refresh().ConfigureAwait(false);
            return p;
        }

        public new async Task Refresh()
        {
            dynamic powerdata = await Execute("emeter", "get_realtime").ConfigureAwait(false);
            CurrentPowerUsage = new PowerData(powerdata , HardwareVersion);
            _gainData = await Execute("emeter", "get_vgain_igain").ConfigureAwait(false);
            await base.Refresh().ConfigureAwait(false);
        }

        /// <summary>
        /// Erases all emeter statistics 
        /// </summary>
        public void EraseStats()
        {
            Task.Run(async () =>
            {
                await Execute("emeter", "erase_emeter_stat").ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Query collected usage statistics from a specific month
        /// </summary>
        /// <returns><c>Dictionary&lt;DateTime, float&gt;</c> of each day in a month and energy consumption of that day in kWh</returns>
        /// <param name = "month" >month of <paramref name="year"/>: ranging from 1(january) to 12(december)</param>
        /// <param name = "year" ></param>
        public async Task<Dictionary<DateTime, float>> GetMonthStats(int month, int year)
        {
            dynamic result = await Execute("emeter", "get_daystat", new JObject
                {
                    new JProperty("month", month),
                    new JProperty("year", year)
                }, null).ConfigureAwait(false);
            var stats = new Dictionary<DateTime, float>();
            foreach (dynamic day_stat in result.day_list)
            {
                stats.Add(new DateTime((int)day_stat.year, (int)day_stat.month, (int)day_stat.day), (float)(day_stat.energy ?? (day_stat.energy_wh / WATTS_IN_KILOWATT)));
            }
            return stats;
        }

        /// <summary>
        /// Query collected usage statistics over the course of a year
        /// </summary>
        /// <returns><c>Dictionary&lt;int, float&gt;</c> of months and energy consumption in kWh</returns>
        /// <param name = "year" >year of stats</param>
        public async Task<Dictionary<int, float>> GetYearStats(int year)
        {
            if (year > DateTime.Now.Year || year < 2010) throw new ArgumentOutOfRangeException($"Can't get stats for {year}. Invalid year!");

            dynamic result = await Execute("emeter", "get_monthstat", "year", year).ConfigureAwait(false);
            var stats = new Dictionary<int, int>();
            foreach (dynamic month_stat in result.month_list)
            {
                stats.Add((int)month_stat.month, (float)(month_stat.energy ?? (month_stat.energy_wh / WATTS_IN_KILOWATT)));
            }
            return stats;
        }
    }
}