using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class JoinChannelResponse
    {
        public ChannelMember member { get; set; }
        public Channel channel { get; set; }

        public static JoinChannelResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new JoinChannelResponse
            {
                member = ChannelMember.FromJson(json["member"]),
                channel = Channel.FromJson(json["channel"]),
            };
        }
    }
}