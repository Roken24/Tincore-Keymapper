using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class Embed
    {
        public EmbedData embedData { get; set; }
        public string embedType { get; set; }

        public static Embed FromJson(JsonValue json)
        {
            return new Embed
            {
                embedData = EmbedData.FromJson(json["embedData"]),
                embedType = json["embedType"],
            };
        }
    }
}