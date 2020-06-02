using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class SearchUserResponse
    {
        public List<User> items { get; set; }
        public int total { get; set; }

        public static SearchUserResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new SearchUserResponse
            {
                items = json["items"].AsJsonArray?
                    .Select(User.FromJson)
                    .ToList(),
                total = json["total"],
            };
        }
    }
}