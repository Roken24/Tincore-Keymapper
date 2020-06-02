using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class Message
    {
        public bool sendError { get; set; }
        
        public string channelId { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string nonce { get; set; }
        public User author { get; set; }
        public List<Attachment> attachments { get; set; }
        public string quoteMessageId { get; set; }
        public string parentMessageId { get; set; }
        public List<Embed> embeds { get; set; }
        public List<User> mentions { get; set; }
        public bool mentionEveryone { get; set; }
        public string deletedTime { get; set; }
        public string lastEditedId { get; set; }

        public static Message FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new Message
            {
                channelId = json["channelId"],
                content = json["content"],
                type = json["type"],
                id = json["id"],
                nonce = json["nonce"],
                author = User.FromJson(json["author"]),
                attachments = json["attachments"].AsJsonArray?
                    .Select(Attachment.FromJson)
                    .ToList(),
                quoteMessageId = json["quoteMessageId"],
                parentMessageId = json["parentMessageId"],
                embeds = json["embeds"].AsJsonArray?
                    .Select(Embed.FromJson)
                    .ToList(),
                mentions = json["mentions"].AsJsonArray?
                    .Select(User.FromJson)
                    .ToList(),
                mentionEveryone = json["mentionEveryone"],
                deletedTime = json["deletedTime"],
                lastEditedId = json["lastEditedId"],
            };
        }
    }
}