using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class TypingFrame : Frame
    {
        public TypingFrameData data { get; set; }

        public new static TypingFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new TypingFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = TypingFrameData.FromJson(json["d"]),
            };
        }
    }

    public class TypingFrameData
    {
        public string channelId { get; set; }
        public long timestamp { get; set; }
        public string userId { get; set; }

        public static TypingFrameData FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new TypingFrameData
            {
                channelId = json["channelId"],
                timestamp = json["timestamp"],
                userId = json["userId"],
            };
        }
    }
}