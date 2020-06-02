using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class Group
    {
        public string id { get; set; }
        public string description { get; set; }
        public string privacy { get; set; }

        public static Group FromJson(JsonValue json)
        {
            return new Group
            {
                id = json["id"],
                description = json["description"],
                privacy = json["privacy"],
            };
        }
    }
}