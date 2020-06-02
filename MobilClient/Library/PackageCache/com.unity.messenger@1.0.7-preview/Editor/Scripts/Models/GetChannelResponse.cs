using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class GetChannelResponse
    {
        public Channel channel { get; set; }
        public Group groupFull { get; set; }

        public static GetChannelResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new GetChannelResponse
            {
                channel = Channel.FromJson(json["channel"]),
                groupFull = Group.FromJson(json["groupFull"]),
            };
        }
    }
}