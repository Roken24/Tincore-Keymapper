using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ChannelDeleteFrame : Frame
    {
        public Channel data { get; set; }

        public new static ChannelDeleteFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new ChannelDeleteFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Channel.FromJson(json["d"]),
            };
        }
    }
}