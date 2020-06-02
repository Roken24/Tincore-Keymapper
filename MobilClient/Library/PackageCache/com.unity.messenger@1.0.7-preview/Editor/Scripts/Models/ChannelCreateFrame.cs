using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ChannelCreateFrame : Frame
    {
        public Channel data { get; set; }

        public new static ChannelCreateFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new ChannelCreateFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Channel.FromJson(json["d"]),
            };
        }
    }
}