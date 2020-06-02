using RSG;
using Unity.UIWidgets.widgets;
using WebSocketSharp;

namespace Unity.Messenger
{
    public static class WebSocketExtension
    {
        public static IPromise AsyncConnect(this WebSocket webSocket)
        {
            return new Promise(isSync: true, resolver: (resolve, reject) =>
            {
                webSocket.OnOpen += (sender, args) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(resolve);
                };
                webSocket.Connect();
            });
        }
    }
}