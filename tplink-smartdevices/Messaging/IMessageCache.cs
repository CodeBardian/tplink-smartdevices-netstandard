using System.Text.Json;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Messaging
{
    public abstract class IMessageCache
    {
        public abstract Task<JsonElement> Request(SmartHomeProtocolMessage message, string hostname, int port = 9999);
    }
}
