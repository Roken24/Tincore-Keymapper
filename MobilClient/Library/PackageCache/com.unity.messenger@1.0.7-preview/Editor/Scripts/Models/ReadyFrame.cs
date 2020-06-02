using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ReadyFrame : Frame
    {
        public ReadyFrameData data { get; set; }

        public new static ReadyFrame FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new ReadyFrame
            {
                opCode = json["op"],
                sequence = json["s"],
                type = json["t"],
                data = ReadyFrameData.FromJson(json["d"]),
            };
        } 
    }
    
    public class ReadyFrameData
    {
        public List<Channel> lobbyChannels { get; set; }
        public List<Message> lastMessages { get; set; }
        public List<User> users { get; set; }
        public string userId { get; set; }
        public List<ReadState> readStates { get; set; }
        public Dictionary<string, Group> groupMap { get; set; }

        public static ReadyFrameData FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }

            Dictionary<string, Group> groupMap = null;
            if (!json["groupMap"].IsNull)
            {
                groupMap = new Dictionary<string, Group>();
                using (var enumerator = json["groupMap"].AsJsonObject.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        groupMap[enumerator.Current.Key] = Group.FromJson(enumerator.Current.Value);
                    }
                }
            }
            
            return new ReadyFrameData
            {
                lobbyChannels = json["lobbyChannels"].AsJsonArray?
                    .Select(Channel.FromJson)
                    .ToList(),
                lastMessages = json["lastMessages"].AsJsonArray?
                    .Select(Message.FromJson)
                    .ToList(),
                users = json["users"].AsJsonArray?
                    .Select(User.FromJson)
                    .ToList(),
                userId = json["userId"],
                readStates = json["readStates"].AsJsonArray?
                    .Select(ReadState.FromJson)
                    .ToList(),
                groupMap = groupMap,
            };
        }
    }
}