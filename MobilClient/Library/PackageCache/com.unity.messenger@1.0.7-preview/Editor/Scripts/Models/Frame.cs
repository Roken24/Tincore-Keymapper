using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public abstract class Frame
    {
        public int opCode { get; set; }
        public int sequence { get; set; }
        public string type { get; set; }

        public static Frame FromJson(JsonValue json)
        {
            var type = json["t"].AsString;
            switch (type)
            {
                case "READY":
                    return ReadyFrame.FromJson(json);
                case "PING":
                    return PingFrame.FromJson(json);
                case "MESSAGE_CREATE":
                    return MessageCreateFrame.FromJson(json);
                case "MESSAGE_DELETE":
                    return MessageDeleteFrame.FromJson(json);
                case "MESSAGE_UPDATE":
                    return MessageUpdateFrame.FromJson(json);
                case "CHANNEL_DELETE":
                    return ChannelDeleteFrame.FromJson(json);
                case "CHANNEL_UPDATE":
                    return ChannelUpdateFrame.FromJson(json);
                case "CHANNEL_CREATE":
                    return ChannelCreateFrame.FromJson(json);
                case "TYPING_START":
                    return TypingFrame.FromJson(json);
                default:
                    return null;
            }
        }
    }
}