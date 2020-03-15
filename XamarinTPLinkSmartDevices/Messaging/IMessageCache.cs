using System.Threading.Tasks;

namespace XamarinTPLinkSmartDevices.Messaging
{
    public abstract class IMessageCache
    {
        public abstract Task<dynamic> Request(SmartHomeProtocolMessage message, string hostname, int port = 9999);
    }
}
