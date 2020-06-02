using System;
using System.Collections.Generic;
using System.IO;
using Unity.UIWidgets.painting;

namespace Unity.Messenger
{
    public partial class Utils
    {
        private static readonly HashSet<string> Fetching = new HashSet<string>();

        public static ImageProvider ProxiedImage(
            string url,
            string cookie = null
        )
        {
            var uriLocalPath = new Uri(url).LocalPath;
            if (uriLocalPath.StartsWith("/"))
            {
                uriLocalPath = uriLocalPath.Substring(1);
            }

            var localPath = Path.Combine(UnityEngine.Application.temporaryCachePath, uriLocalPath);
            lock (Fetching)
            {
                if (!Fetching.Contains(localPath) && File.Exists(localPath))
                {
                    return new FileImage(localPath);
                }

                Fetching.Add(localPath);
            }

            var client = new System.Net.WebClient();
            if (cookie != null)
            {
                client.Headers.Add("Cookie", cookie);
            }
            
            client.DownloadFileCompleted += (sender, args) =>
            {
                lock (Fetching)
                {
                    Fetching.Remove(localPath);
                }
            };
            client.DownloadFileAsync(new Uri(url), localPath);
            
            if (cookie != null)
            {
                return new NetworkImage(
                    url,
                    headers: new Dictionary<string, string>
                    {
                        ["Cookie"] = cookie,
                    }
                );
            }

            return new NetworkImage(url);
        }
    }
}