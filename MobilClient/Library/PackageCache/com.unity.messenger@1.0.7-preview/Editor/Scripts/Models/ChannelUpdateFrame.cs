using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ChannelUpdateFrame : Frame
    {
        public Channel data { get; set; }

        public new static ChannelUpdateFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new ChannelUpdateFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Channel.FromJson(json["d"]),
            };
        }
    }
}