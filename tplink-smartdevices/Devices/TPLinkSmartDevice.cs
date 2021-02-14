using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TPLinkSmartDevices.Messaging;

namespace TPLinkSmartDevices.Devices
{
    public abstract class TPLinkSmartDevice
    {
        const byte INITIALIZATION_VECTOR = 171;

        public string Hostname { get; protected set; }
        public int Port { get; protected set; }

        public IMessageCache MessageCache { get; set; } = new TimeGatedMessageCache(2);

        public string SoftwareVersion { get; private set; }
        public string HardwareVersion { get; private set; }
        public string Type { get; private set; }
        public string Model { get; private set; }
        public string MacAddress { get; private set; }
        public string DevName { get; private set; }
        public string HardwareId { get; private set; }
        public string FirmwareId { get; private set; }
        public string DeviceId { get; private set; }
        public string OemId { get; private set; }
        public string CloudServer { get; private set; }
        public bool RemoteAccessEnabled { get; set; }
        public int RSSI { get; private set; } 
        public double[] LocationLatLong { get; private set; }

        public string Alias { get; private set; }

        private DateTime CurrentTime {get;  set; }

        protected TPLinkSmartDevice(string hostname, int port=9999)
        {
            Hostname = hostname;
            Port = port;
        }

        protected TPLinkSmartDevice() { }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public async Task Refresh(dynamic sysInfo = null)
        {
            await GetCloudInfo().ConfigureAwait(false);
            if (sysInfo == null)
                sysInfo = await Execute("system", "get_sysinfo").ConfigureAwait(false);

            SoftwareVersion = sysInfo.sw_ver;
            HardwareVersion = sysInfo.hw_ver;
            Type = sysInfo.type;
            Model = sysInfo.model;
            MacAddress = sysInfo.mac;
            DevName = sysInfo.dev_name;
            Alias = sysInfo.alias;
            HardwareId = sysInfo.hwId;
            FirmwareId = sysInfo.fwId;
            DeviceId = sysInfo.deviceId;
            OemId = sysInfo.oemId;
            RSSI = sysInfo.rssi;

            if (sysInfo.latitude != null)
                LocationLatLong = new double[2] { sysInfo.latitude, sysInfo.longitude };
            else if (sysInfo.latitude_i != null)
                LocationLatLong = new double[2] { sysInfo.latitude_i, sysInfo.longitude_i };
        }

        /// <summary>
        /// Sends command to device and returns answer 
        /// </summary>
        protected async Task<dynamic> Execute(string system, string command, object argument = null, object value = null)
        {
            var message = new SmartHomeProtocolMessage(system, command, argument, value);
            return await MessageCache.Request(message, Hostname, Port).ConfigureAwait(false);
        }

        protected async Task<dynamic> Execute(string json)
        {
            var message = new SmartHomeProtocolMessage(json);
            return await MessageCache.Request(message, Hostname, Port);
        }

        public async Task SetAlias(string value)
        {
            await Execute("system", "set_dev_alias", "alias", value).ConfigureAwait(false);
            this.Alias = value;
        }

        public async Task<DateTime> GetTime()
        {
            dynamic rawTime = await Execute("time", "get_time").ConfigureAwait(false);
            return new DateTime((int)rawTime.year, (int)rawTime.month, (int)rawTime.mday, (int)rawTime.hour, (int)rawTime.min, (int)rawTime.sec);
        }

        public async Task GetCloudInfo()
        {
            dynamic cloudInfo = await Execute("cnCloud", "get_info").ConfigureAwait(false);
            CloudServer = (string)cloudInfo.server;
            RemoteAccessEnabled = Convert.ToBoolean((int)cloudInfo.binded);
        }

        /// <summary>
        /// Binds account to cloud server
        /// </summary>
        public async Task ConfigureRemoteAccess(string username, string password)
        {
            if (!RemoteAccessEnabled) await SetRemoteAccessEnabled(true).ConfigureAwait(false);
            try
            {               
                dynamic result = await Execute("cnCloud", "bind", new JObject
                    {
                        new JProperty("username", username),
                        new JProperty("password", password)
                    }, null).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                RemoteAccessEnabled = false;
                if (e.Message.Contains("20601") || e.Message.Contains("3"))
                {
                    throw new Exception("The specified password is incorrect");
                }
                else if (e.Message.Contains("20600"))
                {
                    throw new Exception("The username wasn't found");
                };
                throw new Exception("Internal error");
            }
        }

        /// <summary>
        /// Unbinds currently set account from cloud server
        /// </summary>
        public async Task UnbindRemoteAccess()
        {
            dynamic result = await Execute("cnCloud", "unbind").ConfigureAwait(false);
            await SetRemoteAccessEnabled(false).ConfigureAwait(false);
        }

        private async Task SetRemoteAccessEnabled(bool enabled, string server = "n-devs.tplinkcloud.com")
        {
            if (enabled)
            {
                dynamic result = await Execute("cnCloud", "set_server_url", "server", server).ConfigureAwait(false);
                RemoteAccessEnabled = true;
            }
            else
            {
                dynamic result = await Execute("cnCloud", "set_server_url", "server", "bogus.server.com").ConfigureAwait(false);
                RemoteAccessEnabled = false;
            }
        }

        public abstract Task SetPoweredOn(bool value);
    }
}
