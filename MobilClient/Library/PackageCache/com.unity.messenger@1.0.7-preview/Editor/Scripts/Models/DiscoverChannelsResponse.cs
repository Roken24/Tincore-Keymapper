using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class DiscoverChannelsResponse
    {
        public List<string> discoverList { get; set; }
        public List<string> joinedList { get; set; }
        public Dictionary<string, Channel> channelMap { get; set; }
        public Dictionary<string, Group> groupFullMap { get; set; }

        public static DiscoverChannelsResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }

            Dictionary<string, Channel> channelMap = null;
            if (!json["channelMap"].IsNull)
            {
                channelMap = new Dictionary<string, Channel>();
                using (var enumerator = json["channelMap"].AsJsonObject.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        channelMap[enumerator.Current.Key] = Channel.FromJson(enumerator.Current.Value);
                    }
                }
            }

            Dictionary<string, Group> groupFullMap = null;
            if (!json["groupFullMap"].IsNull)
            {
                groupFullMap = new Dictionary<string, Group>();
                using (var enumerator = json["groupFullMap"].AsJsonObject.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        groupFullMap[enumerator.Current.Key] = Group.FromJson(enumerator.Current.Value);
                    }
                }
            }
            
            return new DiscoverChannelsResponse
            {
                discoverList = json["discoverList"].AsJsonArray?
                    .Select(v => (string) v)
                    .ToList(),
                joinedList = json["joinedList"].AsJsonArray?
                    .Select(v => (string) v)
                    .ToList(),
                channelMap = channelMap,
                groupFullMap = groupFullMap,
            };
        }
    }
}