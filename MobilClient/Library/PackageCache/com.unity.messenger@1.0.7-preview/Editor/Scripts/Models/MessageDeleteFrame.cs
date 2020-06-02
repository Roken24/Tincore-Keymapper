using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class MessageDeleteFrame : Frame
    {
        public Message data { get; set; }
        public new static MessageDeleteFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new MessageDeleteFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Message.FromJson(json["d"]),
            };
        }
    }
}