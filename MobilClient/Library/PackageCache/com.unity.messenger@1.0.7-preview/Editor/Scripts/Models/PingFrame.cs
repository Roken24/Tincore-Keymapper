using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class PingFrame : Frame
    {
        public PingFrameData data { get; set; }

        public new static PingFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new PingFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = PingFrameData.FromJson(json["d"]),
            };
        }
    }

    public class PingFrameData
    {
        public long timeStamp { get; set; }
        public string currentChannelId { get; set; }

        public static PingFrameData FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new PingFrameData
            {
                timeStamp = json["ts"],
                currentChannelId = json["cid"],
            };
        }
    }
}