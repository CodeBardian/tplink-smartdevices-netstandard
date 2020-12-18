using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Devices
{
    class TPLinkSmartDimmer : TPLinkSmartDevice
    {
        private DimmerOptions _options;

        [Obsolete("Use async factory method TPLinkSmartDimmer.Create() instead")]
        public TPLinkSmartDimmer(string hostName, int port = 9999) : base(hostName, port)
        {
            Task.Run(async () => await Refresh()).GetAwaiter().GetResult();
        }

        private TPLinkSmartDimmer() { }

        public static async Task<TPLinkSmartDimmer> Create(string hostname, int port = 9999, DimmerOptions opts = null)
        {
            var d = new TPLinkSmartDimmer() { Hostname = hostname, Port = port };
            d._options = opts ?? new DimmerOptions();
            await d.Refresh().ConfigureAwait(false);
            return d;
        }

        public async Task Refresh()
        {
            dynamic sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);

            await RetrievePresets();
            await Refresh((object)sysInfo).ConfigureAwait(false);
        }

        private Task RetrievePresets()
        {
            throw new NotImplementedException();
        }
    }
}
