using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class Attachment
    {
        public string contentType { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string signedUrl { get; set; }
        public string filename { get; set; }
        public int size { get; set; }
        
        public static Attachment FromJson(JsonValue json)
        {
            return new Attachment
            {
                contentType = json["contentType"],
                url = json["url"],
                id = json["id"],
                height = json["height"],
                width = json["width"],
                signedUrl = json["signedUrl"],
                filename = json["filename"],
                size = json["size"],
            };
        }
        
        public bool local { get; set; }
        public System.Func<float> progress { get; set; }
    }
}