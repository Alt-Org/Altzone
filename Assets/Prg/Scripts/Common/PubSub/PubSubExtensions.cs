using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.PubSub
{
    public static class PubSubExtensions
    {
        private static readonly Hub Hub = new Hub();
        private static bool _isApplicationQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            SetEditorStatus();
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetEditorStatus()
        {
            void CheckHandlerCount()
            {
                if (_isApplicationQuitting)
                {
                    return;
                }
                var handlerCount = Hub.Handlers.Count;
                if (handlerCount <= 0)
                {
                    return;
                }
                foreach (var h in Hub.Handlers)
                {
                    Debug.Log($"handler {h}");
                }
                Debug.LogWarning($"sceneUnloaded PubSubExtensions.HandlerCount is {handlerCount}");
            }

            _isApplicationQuitting = false;
            Application.quitting += () => _isApplicationQuitting = true;
            SceneManager.sceneUnloaded += _ => CheckHandlerCount();
        }

        public static bool Exists<T>(this object subscriber)
        {
            return Hub.Exists<T>(subscriber);
        }

        public static void Publish<T>(this object _)
        {
            Hub.Publish(default(T));
        }

        public static void Publish<T>(this object _, T data)
        {
            Hub.Publish(data);
        }

        public static void Subscribe<T>(this object subscriber, Action<T> handler)
        {
            Hub.Subscribe(subscriber, handler);
        }

        public static void Unsubscribe(this object subscriber)
        {
            Hub.Unsubscribe(subscriber);
        }

        public static void Unsubscribe<T>(this object subscriber)
        {
            Hub.Unsubscribe(subscriber, (Action<T>)null);
        }

        public static void Unsubscribe<T>(this object subscriber, Action<T> handler)
        {
            Hub.Unsubscribe(subscriber, handler);
        }
    }
}