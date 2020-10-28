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
        /// <returns><c>Dictionary&lt;DateTime, float&gt;</c> of each day in a month and energy consumption of that day in kWh</returns>
        /// <param name = "month" >month of <paramref name="year"/>: ranging from 1(january) to 12(december)</param>
        /// <param name = "year" ></param>
        public async Task<Dictionary<DateTime, float>> GetMonthStats(int month, int year)
        {
            dynamic result = await Execute("emeter", "get_daystat", new JObject
                {
                    new JProperty("month", month),
                    new JProperty("year", year)
                }, null);
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
            //TODO: check if year is correct
            dynamic result = await Execute("emeter", "get_monthstat", "year", year);
            var stats = new Dictionary<int, float>();
            foreach (dynamic month_stat in result.month_list)
            {
                stats.Add((int)month_stat.month, (float)(month_stat.energy ?? (month_stat.energy_wh / WATTS_IN_KILOWATT)));
            }
            return stats;
        }
    }
}