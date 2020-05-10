using Newtonsoft.Json.Linq;
using System;
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
                CurrentPowerUsage =  new PowerData(await Execute("emeter", "get_realtime"));
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

        public async void GetDayStats()
        {
            dynamic result = await Execute("emeter", "get_daystat", new JObject
                {
                    new JProperty("month", 3),
                    new JProperty("password", 2020)
                }, null);
            result = null;
        }
    }
}