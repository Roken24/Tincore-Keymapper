using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ReadState
    {
        public string channelId { get; set; }
        public string lastMentionId { get; set; }
        public string lastMessageId { get; set; }
        public int mentionCount { get; set; }

        public static ReadState FromJson(JsonValue json)
        {
            return new ReadState
            {
                channelId = json["channelId"],
                lastMentionId = json["lastMentionId"],
                lastMessageId = json["lastMessageId"],
                mentionCount = json["mentionCount"],
            };
        } 
    }
}