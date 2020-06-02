using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class User
    {
        public string id { get; set; }
        public string fullName { get; set; }
        public string avatar { get; set; }
        public string title { get; set; }

        public static User FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new User
            {
                id = json["id"],
                fullName = json["fullname"],
                avatar = json["avatar"],
                title = json["title"],
            };
        }

        public JsonValue ToJson()
        {
            return new JsonObject()
                .Add("id", id)
                .Add("fullname", fullName)
                .Add("avatar", avatar)
                .Add("title", title);
        }
    }
}