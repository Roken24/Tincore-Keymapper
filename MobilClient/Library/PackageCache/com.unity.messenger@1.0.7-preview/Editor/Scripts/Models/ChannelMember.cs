using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ChannelMember
    {
        public User user { get; set; }
        public string role { get; set; }

        public static ChannelMember FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new ChannelMember
            {
                user = User.FromJson(json["user"]),
                role = json["role"],
            };
        }
    }
}