using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ListLobbyResponse
    {
        public int currentPage { get; set; }
        public List<Channel> items { get; set; }
        public List<int> pages { get; set; }
        public int total { get; set; }

        public static ListLobbyResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }

            return new ListLobbyResponse
            {
                currentPage = json["currentPage"],
                items = json["items"].AsJsonArray?
                    .Select(Channel.FromJson)
                    .ToList(),
                pages = json["pages"].AsJsonArray?
                    .Select(v => (int) v)
                    .ToList(),
                total = json["total"],
            };
        }
    }
}