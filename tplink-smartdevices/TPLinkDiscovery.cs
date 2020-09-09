using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TPLinkSmartDevices.Devices;
using TPLinkSmartDevices.Events;
using TPLinkSmartDevices.Messaging;

namespace TPLinkSmartDevices
{
    public class TPLinkDiscovery
    {
        public event EventHandler<DeviceFoundEventArgs> DeviceFound;

        private int PORT_NUMBER = 9999;

        public List<TPLinkSmartDevice> DiscoveredDevices { get; }

        private bool discoveryComplete;

        public TPLinkDiscovery()
        {
            DiscoveredDevices = new List<TPLinkSmartDevice>();
        }

        public async Task<List<TPLinkSmartDevice>> DiscoverAsync(int port = 9999, int timeout = 5000)
        {
            discoveryComplete = false;
            DiscoveredDevices.Clear();
            PORT_NUMBER = port;
            await SendDiscoveryRequestAsync().ConfigureAwait(false);
            await ReceiveAsync().ConfigureAwait(false);
            return DiscoveredDevices;
        }

        private async Task ReceiveAsync()
        {
            if (discoveryComplete) //Prevent ObjectDisposedException/NullReferenceException when the Close() function is called
            {
                return;
            }

            var udpListener = new UdpClient(PORT_NUMBER) { EnableBroadcast = true };
            UdpReceiveResult response = await udpListener.ReceiveAsync().ConfigureAwait(false);
            IPEndPoint ip = response.RemoteEndPoint;
            var message = Encoding.ASCII.GetString(SmartHomeProtocolEncoder.Decrypt(response.Buffer));

            try
            {
                TPLinkSmartDevice device = null;
                dynamic sys_info = ((dynamic)JObject.Parse(message)).system.get_sysinfo;
                string model = (string)sys_info.model;

                if (model != null)
                {
                    if (model.StartsWith("HS110"))
                    {
                        device = new TPLinkSmartMeterPlug(ip.Address.ToString());
                    }
                    else if (model.StartsWith("HS"))
                    {
                        device = new TPLinkSmartPlug(ip.Address.ToString());
                    }
                    else if (model.StartsWith("KL") || model.StartsWith("LB"))
                    {
                        device = new TPLinkSmartBulb(ip.Address.ToString());
                    }

                    // new device found, store in list and raise event
                    if (device != null)
                    {
                        DiscoveredDevices.Add(device);
                        OnDeviceFound(device);
                    }
                }

            }
            catch (RuntimeBinderException ex)
            {
                //discovered wrong device
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                // ensure the socket is closed even if exception happenned
                udpListener.Dispose();
            }

            discoveryComplete = true;
        }

        private async Task SendDiscoveryRequestAsync()
        {
            using (UdpClient client = new UdpClient(PORT_NUMBER) { EnableBroadcast = true })
            {
                var discoveryJson = JObject.FromObject(new
                {
                    system = new { get_sysinfo = (object)null },
                    emeter = new { get_realtime = (object)null }
                }).ToString(Newtonsoft.Json.Formatting.None);
                var discoveryPacket = SmartHomeProtocolEncoder.Encrypt(discoveryJson).ToArray();

                var bytes = discoveryPacket.Skip(4).ToArray();
                int sentBytes = await client.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, PORT_NUMBER)).ConfigureAwait(false);

                // no bytes sents
                if (sentBytes == 0)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private void OnDeviceFound(TPLinkSmartDevice device)
        {
            DeviceFound?.Invoke(this, new DeviceFoundEventArgs(device));
        }

        /// <summary>
        /// Makes device connect to specified network. Host who runs the application needs to be connected to the open configuration network! (TP-Link_Smart Plug_XXXX or similar)
        /// </summary>
        public async Task Associate(string ssid, string password, int type = 3)
        {
            dynamic scan = await new SmartHomeProtocolMessage("netif", "get_scaninfo", "refresh", "1").Execute("192.168.0.1", 9999);
            //TODO: use scan to get type of user specified network

            if (scan == null || !scan.ToString().Contains(ssid))
            {
                throw new Exception("Couldn't find network!");
            }

            dynamic result = await new SmartHomeProtocolMessage("netif", "set_stainfo", new JObject
            {
                new JProperty("ssid", ssid),
                new JProperty("password", password),
                new JProperty("key_type", type)
            }, null).Execute("192.168.0.1", 9999);

            if (result == null)
            {
                throw new Exception("Couldn't connect to network. Check password");
            }
            else if (result["err_code"] != null && result.err_code != 0)
            {
                throw new Exception($"Protocol error {result.err_code} ({result.err_msg})");
            }
        }
    }
}
