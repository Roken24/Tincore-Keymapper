using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class GetMessagesResponse
    {
        public bool hasMore { get; set; }
        public bool hasMoreNew { get; set; }
        public List<Message> items { get; set; }
        public List<Message> parents { get; set; }

        public static GetMessagesResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new GetMessagesResponse
            {
                hasMore = json["hasMore"],
                hasMoreNew = json["hasMoreNew"],
                items = json["items"].AsJsonArray?
                    .Select(Message.FromJson)
                    .ToList(),
                parents = json["parents"].AsJsonArray?
                    .Select(Message.FromJson)
                    .ToList(),
            };
        }
    }
}