using Newtonsoft.Json;

namespace LiskMasterWallet.Types
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class WSMessage
    {
        public string messageType;
        public object payload;
    }
}
