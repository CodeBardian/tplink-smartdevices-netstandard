using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XamarinTPLinkSmartDevices.Devices;
using Microsoft.CSharp.RuntimeBinder;

namespace XamarinTPLinkSmartDevices
{
    public class TPLinkDiscovery
    {
        private int PORT_NUMBER = 9999;

        public List<TPLinkSmartDevice> DiscoveredDevices { get; private set; }

        private UdpClient udp;

        private bool discoveryComplete = false;

        public TPLinkDiscovery()
        {
            DiscoveredDevices = new List<TPLinkSmartDevice>();
        }

        public async Task<List<TPLinkSmartDevice>> Discover(int port=9999, int timeout=10000)
        {
            discoveryComplete = false;

            DiscoveredDevices.Clear();
            PORT_NUMBER = port;

            SendDiscoveryRequestAsync();
            
            udp = new UdpClient(PORT_NUMBER);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, PORT_NUMBER);
            udp.EnableBroadcast = true;
            Receive();

            return await Task.Delay(timeout).ContinueWith(t =>
            {
                discoveryComplete = true;
                udp = null;

                return DiscoveredDevices;
            });
        }

        private async void Receive()
        {
            if (discoveryComplete) //Prevent ObjectDisposedException/NullReferenceException when the Close() function is called
                return;

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, PORT_NUMBER);
            UdpReceiveResult result = await udp.ReceiveAsync();
            ip = result.RemoteEndPoint;
            var message = Encoding.ASCII.GetString(Messaging.SmartHomeProtocolEncoder.Decrypt(result.Buffer));

            TPLinkSmartDevice device = null;
            try
            {
                dynamic sys_info = ((dynamic)JObject.Parse(message)).system.get_sysinfo;
                string model = (string)sys_info.model;
                if (model != null)
                {
                    if (model.StartsWith("HS110"))
                        device = new TPLinkSmartMeterPlug(ip.Address.ToString());
                    else if (model.StartsWith("HS"))
                        device = new TPLinkSmartPlug(ip.Address.ToString());
                    else if (model.StartsWith("KL") || model.StartsWith("LB"))
                        device = new TPLinkSmartBulb(ip.Address.ToString());
                }
            }
            catch (RuntimeBinderException)
            {
                //discovered wrong device
            }

            if (device != null)
                DiscoveredDevices.Add(device);
            if (udp != null)
                Receive();
        }

        private async void SendDiscoveryRequestAsync()
        {
            UdpClient client = new UdpClient(PORT_NUMBER);
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, PORT_NUMBER);

            var discoveryJson = JObject.FromObject(new
            {
                system = new { get_sysinfo = (object)null },
                emeter = new { get_realtime = (object)null }
            }).ToString(Newtonsoft.Json.Formatting.None);
            var discoveryPacket = Messaging.SmartHomeProtocolEncoder.Encrypt(discoveryJson).ToArray();

            var bytes = discoveryPacket.Skip(4).ToArray();
            client.EnableBroadcast = true;
            await client.SendAsync(bytes, bytes.Length, ip);
            client.Dispose();
        }
    }
}
