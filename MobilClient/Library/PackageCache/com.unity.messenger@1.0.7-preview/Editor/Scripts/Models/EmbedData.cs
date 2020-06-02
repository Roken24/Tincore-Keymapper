using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class EmbedData
    {
        public string description { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string title { get; set; } 
        public string url { get; set; }

        public static EmbedData FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new EmbedData
            {
                description = json["description"],
                image = json["image"],
                name = json["name"],
                title = json["title"],
                url = json["url"],
            };
        }
    }
}