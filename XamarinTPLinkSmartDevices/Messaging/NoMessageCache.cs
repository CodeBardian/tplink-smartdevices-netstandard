using System.Threading.Tasks;

namespace TPLinkSmartDevices.Messaging
{
    public class NoMessageCache : IMessageCache
    {
        public override async Task<dynamic> Request(SmartHomeProtocolMessage message, string hostname, int port = 9999)
        {
            return await message.Execute(hostname, port);
        }
    }
}
