using System.Text.RegularExpressions;
using Unity.Messenger.Components;

namespace Unity.Messenger
{
    public partial class Utils
    {
        private static readonly Regex LocalHandledChannelUrlRegex = new Regex(
            "^https://connect.unity.com/mconnect/channels/([0-9a-f]{16})/?$"
        );

        public static void Launch(string url)
        {
            if (LocalHandledChannelUrlRegex.IsMatch(url))
            {
                var match = LocalHandledChannelUrlRegex.Match(url);
                var channelId = match.Groups[1].Value;
                if (Window.Channels.ContainsKey(channelId))
                {
                    HomePage.currentState?.Select(channelId);
                }
                else
                {
                    HomePage.currentState?.ShowChannelBrief(channelId);
                }
                return;
            }
            UnityEngine.Application.OpenURL(url);
        }
    }
}