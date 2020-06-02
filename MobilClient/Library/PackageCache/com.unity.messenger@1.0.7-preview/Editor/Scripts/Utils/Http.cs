using System;
using System.Collections.Generic;
using System.Text;
using RSG;
using Unity.Messenger.Json;
using Unity.UIWidgets.foundation;
using UnityEngine.Networking;

namespace Unity.Messenger
{
    public partial class Utils
    {
        public static void Get<T>(
            string url,
            Action<T> action)
        {
            Get<T>(url).Then(action);
        }

        public static IPromise<T> Get<T>(
            string url)
        {
            return new Promise<T>(isSync: true, resolver: (resolve, reject) =>
            {
                var session = Window.loginSession;
                var request = UnityWebRequest.Get($"https://connect.unity.com{url}");
                request.SetRequestHeader("X-Requested-With", "XMLHTTPREQUEST");
                if (session != null && session.isNotEmpty())
                {
                    request.SetRequestHeader("Cookie", $"LS={session};");
                }

                request.SendWebRequest().completed += operation =>
                {
                    var content = DownloadHandlerBuffer.GetContent(request);
                    var fromProc = typeof(T).GetMethod("FromJson");
                    var response = (T) fromProc.Invoke(null, new object[] {JsonValue.Parse(content)});
                    resolve(response);
                };
            });
        }

        public static void Post<T>(
            string url,
            string data,
            Action<T> action
        )
        {
            Post<T>(
                url,
                data
            ).Then(action);
        }

        public static IPromise<T> Post<T>(
            string url,
            string data)
        {
            return new Promise<T>(isSync: true, resolver: (resolve, reject) =>
            {
                
                var session = Window.loginSession;
                var request = new UnityWebRequest(
                    $"https://connect.unity.com{url}",
                    UnityWebRequest.kHttpVerbPOST
                )
                {
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                if (data != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
                }
                request.SetRequestHeader("X-Requested-With", "XMLHTTPREQUEST");
                request.SetRequestHeader("Content-Type", "application/json");
                if (session != null && session.isNotEmpty())
                {
                    request.SetRequestHeader("Cookie", $"LS={session};");
                }

                request.SendWebRequest().completed += operation =>
                {
                    var content = DownloadHandlerBuffer.GetContent(request);
                    var fromProc = typeof(T).GetMethod("FromJson");
                    var response = (T) fromProc.Invoke(null, new object[] {JsonValue.Parse(content)});
                    resolve(response);
                };
            });
        }

        public static void PostForm<T>(
            string url,
            List<IMultipartFormSection> formSections,
            Action<T> action,
            out Func<float> progress)
        {
            PostForm<T>(
                url,
                formSections,
                out progress
            ).Then(action);
        }

        public static IPromise<T> PostForm<T>(
            string url,
            List<IMultipartFormSection> formSections,
            out Func<float> progress)
        {
            var session = Window.loginSession;
            var request = UnityWebRequest.Post(
                $"https://connect.unity.com{url}",
                formSections
            );
            request.SetRequestHeader("X-Requested-With", "XMLHTTPREQUEST");
            if (session != null && session.isNotEmpty())
            {
                request.SetRequestHeader("Cookie", $"LS={session};");
            }
            progress = () => request.uploadProgress;
            
            return new Promise<T>(isSync: true, resolver: (resolve, reject) =>
            {
                request.SendWebRequest().completed += operation =>
                {
                    var content = DownloadHandlerBuffer.GetContent(request);
                    var fromProc = typeof(T).GetMethod("FromJson");
                    var response = (T) fromProc.Invoke(null, new object[] {JsonValue.Parse(content)});
                    resolve(response);
                };
                
            });
        }
    }
}