using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class MessageCreateFrame : Frame
    {
        public Message data { get; set; }
        public new static MessageCreateFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new MessageCreateFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Message.FromJson(json["d"]),
            };
        }
    }
}