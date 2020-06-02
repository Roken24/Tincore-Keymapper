using System.Collections.Generic;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class Channel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string workspaceId { get; set; }
        public string thumbnail { get; set; }
        public string name { get; set; }
        public int memberCount { get; set; }
        public string topic { get; set; }
        public Message lastMessage { get; set; }
        public string lastMessageId { get; set; }
        public string liveChannelId { get; set; }
        public bool isMute { get; set; }
        public string groupId { get; set; }
        public string stickTime { get; set; }
        public Dictionary<string, Group> groupMap { get; set; }

        public static Channel FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }

            Dictionary<string, Group> groupMap = null;
            if (!json["groupMap"].IsNull)
            {
                using (var cursor = json["groupMap"].AsJsonObject.GetEnumerator())
                {
                    groupMap = new Dictionary<string, Group>();
                    while (cursor.MoveNext())
                    {
                        groupMap[cursor.Current.Key] = Group.FromJson(cursor.Current.Value);
                    }
                }
            }
            
            return new Channel
            {
                id = json["id"],
                type = json["type"],
                workspaceId = json["workspaceId"],
                thumbnail = json["thumbnail"],
                name = json["name"],
                memberCount = json["memberCount"],
                topic = json["topic"],
                lastMessage = Message.FromJson(json["lastMessage"]),
                lastMessageId = json["lastMessageId"],
                liveChannelId = json["liveChannelId"],
                isMute = json["isMute"],
                groupId = json["groupId"],
                stickTime = json["stickTime"],
                groupMap = groupMap,
            };
        }
    }
}