using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.PubSub
{
    public static class PubSubExtensions
    {
        private static readonly Hub Hub = new Hub();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            InstallListeners();
        }

        [Conditional("FORCE_LOG"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        private static void InstallListeners()
        {
            void CheckHandlerCount()
            {
                var handlerCount = Hub.handlers.Count;
                if (handlerCount > 0)
                {
                    foreach (var h in Hub.handlers)
                    {
                        Debug.Log($"handler {h}");
                    }
                    Debug.LogWarning($"PubSubExtensions.HandlerCount is {handlerCount}");
                }
            }

            SceneManager.sceneUnloaded += s => CheckHandlerCount();
        }

        public static bool Exists<T>(this object obj)
        {
            foreach (var h in Hub.handlers)
            {
                if (Equals(h.Sender.Target, obj) &&
                    typeof(T) == h.Type)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Publish<T>(this object obj)
        {
            Hub.Publish(obj, default(T));
        }

        public static void Publish<T>(this object obj, T data)
        {
            Hub.Publish(obj, data);
        }

        public static void Subscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Subscribe(obj, handler);
        }

        public static void Unsubscribe(this object obj)
        {
            Hub.Unsubscribe(obj);
        }

        public static void Unsubscribe<T>(this object obj)
        {
            Hub.Unsubscribe(obj, (Action<T>)null);
        }

        public static void Unsubscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Unsubscribe(obj, handler);
        }
    }
}