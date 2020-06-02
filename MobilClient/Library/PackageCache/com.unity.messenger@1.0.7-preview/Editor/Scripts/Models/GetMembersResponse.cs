using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Json;

namespace Unity.Messenger.Models
{
    public class GetMembersResponse
    {
        public List<ChannelMember> list { get; set; }
        public int total { get; set; }

        public static GetMembersResponse FromJson(JsonValue json)
        {
            if (json.IsNull)
            {
                return null;
            }
            return new GetMembersResponse
            {
                list = json["list"].AsJsonArray?
                    .Select(ChannelMember.FromJson)
                    .ToList(),
                total = json["total"],
            };
        }
    }
}