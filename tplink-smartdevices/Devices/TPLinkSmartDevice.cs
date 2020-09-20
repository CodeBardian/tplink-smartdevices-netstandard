using System;
using System.Text.Json;
using System.Threading.Tasks;
using TPLinkSmartDevices.Messaging;

namespace TPLinkSmartDevices.Devices
{
    public class TPLinkSmartDevice
    {
        const byte INITIALIZATION_VECTOR = 171;

        public string Hostname { get; protected set; }
        public int Port { get; protected set; }

        public IMessageCache MessageCache { get; } = new TimeGatedMessageCache(2);

        public string SoftwareVersion { get; protected set; }
        public string HardwareVersion { get; protected set; }
        public string Type { get; protected set; }
        public string Model { get; protected set; }
        public string MacAddress { get; protected set; }
        public string DevName { get; protected set; }
        public string HardwareId { get; protected set; }
        public string FirmwareId { get; protected set; }
        public string DeviceId { get; protected set; }
        public string OemId { get; protected set; }
        public string CloudServer { get; protected set; }
        public bool RemoteAccessEnabled { get; protected set; }
        public int RSSI { get; protected set; }
        public double[] LocationLatLong { get; protected set; }

        public string Alias { get; protected set; }

        protected DateTime CurrentTime { get; set; }

        protected TPLinkSmartDevice(string hostname, int port = 9999)
        {
            Hostname = hostname;
            Port = port;
        }

        /// <summary>
        /// Refresh device information
        /// </summary>
        public virtual async Task RefreshAsync(dynamic sysInfo)
        {
            //await GetCloudInfo().ConfigureAwait(false);
            if (sysInfo == null)
            {
                sysInfo = await ExecuteAsync("system", "get_sysinfo").ConfigureAwait(false);
            }

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
            Model = sysInfo.model;

            if (sysInfo.latitude != null)
            {
                LocationLatLong = new double[2] { sysInfo.latitude, sysInfo.longitude };
            }
            else if (sysInfo.latitude_i != null)
            {
                LocationLatLong = new double[2] { sysInfo.latitude_i, sysInfo.longitude_i };
            }
        }

        /// <summary>
        /// Sends command to device and returns answer 
        /// </summary>
        protected async Task<JsonElement> ExecuteAsync(string system, string command, object argument = null, object value = null)
        {
            var message = new SmartHomeProtocolMessage(system, command, argument, value);
            JsonElement result = await MessageCache.Request(message, Hostname, Port);

            return result;
            //return JsonDocument.Parse(result).RootElement;
        }

        public Task SetAlias(string value)
        {
            Alias = value;
            return ExecuteAsync("system", "set_dev_alias", "alias", value);
        }

        public async Task<DateTime> GetTimeAsync()  //refactor needed
        {
            var rawTime = await ExecuteAsync("time", "get_time");
            int year = rawTime.GetProperty("year").GetInt32();
            int month = rawTime.GetProperty("month").GetInt32();
            int day = rawTime.GetProperty("day").GetInt32(); // todo: use mday?
            int hour = rawTime.GetProperty("hour").GetInt32();
            int min = rawTime.GetProperty("min").GetInt32();
            int sec = rawTime.GetProperty("sec").GetInt32();
            return new DateTime(year, month, day, hour, min, sec);
        }

        public async Task GetCloudInfo()
        {
            try
            {
                var cloudInfo = await ExecuteAsync("cnCloud", "get_info").ConfigureAwait(false);
                CloudServer = cloudInfo.GetProperty("server").GetString();
                RemoteAccessEnabled = cloudInfo.GetProperty("binded").GetBoolean();
            }
            catch (Exception)
            {
                RemoteAccessEnabled = false;
            }
        }

        /// <summary>
        /// Binds account to cloud server
        /// </summary>
        public async Task ConfigureRemoteAccess(string username, string password)
        {
            if (!RemoteAccessEnabled)
            {
                await SetRemoteAccessEnabledAsync(true).ConfigureAwait(false);
            }

            try
            {
                await ExecuteAsync("cnCloud", "bind", new
                {
                    username = username,
                    password = password
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
            await ExecuteAsync("cnCloud", "unbind").ConfigureAwait(false);
            await SetRemoteAccessEnabledAsync(false);
        }

        private async Task SetRemoteAccessEnabledAsync(bool enabled, string server = "n-devs.tplinkcloud.com")
        {
            if (enabled)
            {
                await ExecuteAsync("cnCloud", "set_server_url", "server", server).ConfigureAwait(false);
                RemoteAccessEnabled = true;
            }
            else
            {
                await ExecuteAsync("cnCloud", "set_server_url", "server", "bogus.server.com").ConfigureAwait(false);
                RemoteAccessEnabled = false;
            }
        }
    }
}
