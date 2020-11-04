using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Messaging
{
    public class ManualMessageCache : IMessageCache
    {
        private List<MessageCacheItem> _cache = new List<MessageCacheItem>();

        public void Flush()
        {
            _cache.Clear();
        }

        public override async Task<dynamic> Request(SmartHomeProtocolMessage message, string hostname, int port = 9999)
        {
            var cachedMessage = _cache.FirstOrDefault(c => c.Matches(message, hostname, port));

            if (cachedMessage != null)
                return cachedMessage;

            var result = await message.Execute(hostname, port).ConfigureAwait(false);
            _cache.Add(new MessageCacheItem(result, hostname, port));
            return result;
        }

        protected class MessageCacheItem
        {
            internal int Hash { get; set; }
            internal string Hostname { get; set; }
            internal int Port { get; set; }
            internal dynamic MessageResult { get; set; }

            internal MessageCacheItem(dynamic messageResult, string hostname, int port)
            {
                MessageResult = messageResult;
                Hostname = hostname;
                Port = port;
            }

            internal bool Matches(SmartHomeProtocolMessage message, string hostname, int port)
            {
                if (Hostname != hostname || Port != port)
                    return false;

                return message.MessageHash == Hash;
            }
        }
    }
}
