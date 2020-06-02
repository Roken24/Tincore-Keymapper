using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class MessageUpdateFrame : Frame
    {
        public Message data { get; set; }
        public new static MessageUpdateFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new MessageUpdateFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = Message.FromJson(json["d"]),
            };
        }
    }
}