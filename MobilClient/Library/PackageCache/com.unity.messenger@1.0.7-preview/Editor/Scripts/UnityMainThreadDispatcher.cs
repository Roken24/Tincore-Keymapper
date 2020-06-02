using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Messenger
{
    public class UnityMainThreadDispatcher
    {
        private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

        private static UnityMainThreadDispatcher _instance;

        public static UnityMainThreadDispatcher Instance()
        {
            return _instance ?? (_instance = new UnityMainThreadDispatcher());
        }

        public void Update()
        {
            lock (ExecutionQueue)
            {
                while (ExecutionQueue.Count > 0)
                {
                    ExecutionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(action.Invoke);
            }
        }
    }
}