using System.Collections.Generic;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class SendMessageRequest
    {
        public SendMessageRequest(
            string content,
            string nonce,
            List<User> mentions,
            string parentMessageId = "",
            string quoteMessageId = null)
        {
            this.content = content;
            this.parentMessageId = parentMessageId;
            this.quoteMessageId = quoteMessageId;
            this.nonce = nonce;
            this.mentions = mentions;
        }
        
        public List<User> mentions { get; private set; }
        public string content { get; private set; }
        public string parentMessageId { get; private set; }
        public string quoteMessageId { get; private set; }
        public string nonce { get; private set; }

        public JsonValue ToJson()
        {
            var ms = new JsonArray();
            mentions?.ForEach(m => { ms.Add(m.ToJson()); });
            return new JsonObject()
                .Add("content", content)
                .Add("nonce", nonce)
                .Add("mentions", ms)
                .Add("parentMessageId", parentMessageId)
                .Add("quoteMessageId", quoteMessageId);
        }
    }
}