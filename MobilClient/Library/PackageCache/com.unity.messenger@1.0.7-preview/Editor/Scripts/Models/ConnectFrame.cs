using System.Collections.Generic;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class ConnectFrame
    {
        public int opCode { get; set; }
        public ConnectFrameData data { get; set; }

        public JsonValue ToJson()
        {
            return new JsonObject()
                .Add("op", opCode)
                .Add("d", data.ToJson());
        }
    }

    public class ConnectFrameData
    {
        public string loginSession { get; set; }
        public string commitId { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public string clientType { get; set; }
        public bool isApp { get; set; }

        public JsonValue ToJson()
        {
            var props = new JsonObject();
            foreach (var keyValuePair in properties)
            {
                // Not sure how to handle this Csharp object.
                props.Add(keyValuePair.Key, keyValuePair.Value.ToString());
            }
            return new JsonObject()
                .Add("ls", loginSession)
                .Add("commitId", commitId)
                .Add("properties", props)
                .Add("clientType", clientType)
                .Add("isApp", isApp);
        }
    }
}