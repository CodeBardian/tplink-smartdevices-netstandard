﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Messaging
{
    public class SmartHomeProtocolMessage
    {
        public int MessageHash
        {
            get
            {
                var data = Encoding.ASCII.GetBytes(JSON);
                unchecked
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;

                    for (int i = 0; i < data.Length; i++)
                        hash = (hash ^ data[i]) * p;

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }

        public string JSON
        {
            get
            {
                object argObject;
                if (Value != null)
                    argObject = new JObject { new JProperty(Argument.ToString(), Value) };
                else
                    argObject = Argument;

                var root = new JObject { new JProperty(System, new JObject { new JProperty(Command, argObject) }) };

                return root.ToString(Newtonsoft.Json.Formatting.None);
            }
        }

        public string System { get; set; }
        public string Command { get; set; }
        public object Argument { get; set; }
        public object Value { get; set; }

        internal SmartHomeProtocolMessage(string system, string command, object argument, object value)
        {
            System = system;
            Command = command;
            Argument = argument;
            Value = value;
        }

        internal async Task<dynamic> Execute(string hostname, int port)
        {
            var messageToSend = SmartHomeProtocolEncoder.Encrypt(JSON);

            var client = new TcpClient();
            client.ConnectAsync(hostname, port).Wait(2000);

            if (!client.Connected)
            {
                return null;
            }

            byte[] packet = new byte[0];
            using (var stream = client.GetStream())
            {
                await stream.WriteAsync(messageToSend, 0, messageToSend.Length);
                //stream.Write(messageToSend, 0, messageToSend.Length);

                int targetSize = 0;
                var buffer = new List<byte>();
                while (true)
                {
                    var chunk = new byte[1024];
                    var bytesReceived = stream.Read(chunk, 0, chunk.Length);

                    if (!buffer.Any())
                    {
                        var lengthBytes = chunk.Take(4).ToArray();
                        if (BitConverter.IsLittleEndian) 
                            lengthBytes = lengthBytes.Reverse().ToArray();
                        targetSize = (int)BitConverter.ToUInt32(lengthBytes, 0);
                    }
                    buffer.AddRange(chunk.Take(bytesReceived));

                    if (buffer.Count == targetSize + 4)
                        break;
                }

                packet = buffer.Skip(4).Take(targetSize).ToArray();
            }
            client.Dispose();

            var decrypted = Encoding.ASCII.GetString(SmartHomeProtocolEncoder.Decrypt(packet)).Trim('\0');

            var subResult = (dynamic)((JObject)JObject.Parse(decrypted)[System])[Command];
            if (subResult["err_code"] != null && subResult.err_code != 0)
                throw new Exception($"Protocol error {subResult.err_code} ({subResult.err_msg})");

            return subResult;
        }
    }
}
