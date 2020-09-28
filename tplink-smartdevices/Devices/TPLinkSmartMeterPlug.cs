using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartMeterPlug : TPLinkSmartPlug
    {
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

        public new async Task Refresh()
        {
            CurrentPowerUsage = new PowerData(await Execute("emeter", "get_realtime"), HardwareVersion);
            _gainData = await Execute("emeter", "get_vgain_igain");
            await base.Refresh();
        }

        /// <summary>
        /// Erases all emeter statistics 
        /// </summary>
        public void EraseStats()
        {
            Task.Run(async () =>
            {
                await Execute("emeter", "erase_emeter_stat");
            });
        }

        /// <summary>
        /// Query collected usage statistics from a specific month
        /// </summary>
        /// <returns><c>Dictionary&lt;DateTime, int&gt;</c> of each day in a month and energy consumption of that day (in watt hours (?))</returns>
        /// <param name = "month" >month of <paramref name="year"/>: ranging from 1(january) to 12(december)</param>
        /// <param name = "year" ></param>
        public async Task<Dictionary<DateTime, int>> GetMonthStats(int month, int year)
        {
            dynamic result = await Execute("emeter", "get_daystat", new JObject
                {
                    new JProperty("month", month),
                    new JProperty("year", year)
                }, null);
            var stats = new Dictionary<DateTime, int>();
            foreach (dynamic day_stat in result.day_list)
            {
                stats.Add(new DateTime((int)day_stat.year, (int)day_stat.month, (int)day_stat.day), (int)day_stat.energy);
            }
            return stats;
        }

        /// <summary>
        /// Query collected usage statistics over the course of a year
        /// </summary>
        /// <returns><c>Dictionary&lt;int, int&gt;</c> of months and energy consumption</returns>
        /// <param name = "year" >year of stats</param>
        public async Task<Dictionary<int, int>> GetYearStats(int year)
        {
            if (year > DateTime.Now.Year || year < 2010) throw new ArgumentOutOfRangeException($"Can't get stats for {year}. Invalid year!");

            dynamic result = await Execute("emeter", "get_monthstat", "year", year);
            var stats = new Dictionary<int, int>();
            foreach (dynamic month_stat in result.month_list)
            {
                stats.Add((int)month_stat.month, (int)month_stat.energy);
            }
            return stats;
        }
    }
}