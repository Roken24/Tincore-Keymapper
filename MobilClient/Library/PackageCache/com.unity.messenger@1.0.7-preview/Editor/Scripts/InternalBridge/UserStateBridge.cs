using UnityEditor.Connect;
using UnityEngine;

namespace Unity.Messenger.Bridge
{
    public static class UserStateBridge
    {
        public static void RegisterEvent()
        {
            _loggedIn = UnityConnect.instance.loggedIn;
            UnityConnect.instance.StateChanged += state =>
            {
                if (_loggedIn != state.loggedIn)
                {
                    if (state.loggedIn)
                    {
                        OnLoggedIn?.Invoke();
                    }
                    else
                    {
                        OnLoggedOut?.Invoke();
                    }

                    _loggedIn = state.loggedIn;
                }
            };
            if (_loggedIn)
            {
                OnLoggedIn?.Invoke();
            }
        }

        private static bool _loggedIn;
        public static event System.Action OnLoggedIn;
        public static event System.Action OnLoggedOut;
    }
}