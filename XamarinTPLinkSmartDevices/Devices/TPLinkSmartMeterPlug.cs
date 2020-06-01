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
                CurrentPowerUsage = new PowerData(await Execute("emeter", "get_realtime"));
                _gainData = await Execute("emeter", "get_vgain_igain");          
            });
        }

        public new async Task Refresh()
        {
            CurrentPowerUsage = new PowerData(await Execute("emeter", "get_realtime"));
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

        public async Task<Dictionary<int, int>> GetYearStats(int year)
        {
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