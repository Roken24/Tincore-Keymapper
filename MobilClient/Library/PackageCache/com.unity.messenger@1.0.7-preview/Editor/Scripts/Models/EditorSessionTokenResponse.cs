using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class EditorSessionTokenResponse
    {
        public string loginSessionToken { get; set; }
        public string userId { get; set; }

        public static EditorSessionTokenResponse FromJson(JsonValue json)
        {
            return new EditorSessionTokenResponse
            {
                loginSessionToken = json["LSToken"],
                userId = json["userId"],
            };
        }
    }
}