using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Messenger.Notification
{
    public delegate void NotificationCallback(string channelId);
    
    public sealed class NotificationManager
    {
        private static NotificationManager _instance = null;
        private static readonly object Padlock = new object();

#if UNITY_EDITOR_WIN
        [DllImport("UnityToastWin", CallingConvention = CallingConvention.Cdecl)]
        private static extern int showToastWin(string username, string msg, string channelId, string filepath, NotificationCallback cb);

#elif UNITY_EDITOR_OSX
        [DllImport("UnityToastMac", CallingConvention = CallingConvention.Cdecl)]
        private static extern int showToastMac(string username, string msg, string msgId, string channelId, string filepath, NotificationCallback cb);
#endif
        
        private NotificationManager()
        {
        }

        public static NotificationManager instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new NotificationManager();
                    }
                    return _instance;
                }
            }
        }

        public int ShowNotification(bool inChannel, string userId, string username, string msg, string msgId, string channelId, string avatar, NotificationCallback cb) {
            if (inChannel && InternalEditorUtility.isApplicationActive && EditorWindow.focusedWindow.GetType() == typeof(Window)) {
                return 0;
            }
            if (string.IsNullOrEmpty(avatar))
            {
#if UNITY_EDITOR_WIN 
                showToastWin(username, msg, channelId, "", cb);
#elif UNITY_EDITOR_OSX
                showToastMac(username, msg, msgId, channelId, "", cb);
#endif
            }
            else
            {
            System.Net.WebClient client = new System.Net.WebClient();
            var filename = "avatar_" + userId + ".jpg";
            var filepath = Path.Combine(UnityEngine.Application.temporaryCachePath + @"/" + filename);
            client.DownloadFileAsync(new Uri(avatar), filepath);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
            {
#if UNITY_EDITOR_WIN 
                showToastWin(username, msg, channelId, filepath, cb);
#elif UNITY_EDITOR_OSX
                showToastMac(username, msg, msgId, channelId, filepath, cb);
#endif
            });
                
            }

            return 0;
        }
    }
}
