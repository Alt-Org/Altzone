using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Prg.Scripts.Common.PubSub
{
    /// <summary>
    /// Simple Publish Subscribe Pattern <c>Hub</c> implementation using <c>UnityEngine.Object</c>.
    /// </summary>
    /// <remarks>
    /// This implementation is for single threaded use only!
    /// </remarks>
    public class UnityHub
    {
        private class Handler
        {
            public readonly Delegate Action;
            public readonly Object Subscriber;
            public readonly Type MessageType;
            private readonly object _selectorWrapper;

            public bool Select<T>(T message)
            {
                if (_selectorWrapper == null)
                {
                    return true;
                }
                if (_selectorWrapper is SelectorWrapper<T> wrapper)
                {
                    return wrapper.Selector(message);
                }
                return false;
            }

            public Handler(Delegate action, Object subscriber, Type messageType, object selectorWrapper)
            {
                Action = action;
                Subscriber = subscriber;
                MessageType = messageType;
                _selectorWrapper = selectorWrapper;
            }

            public override string ToString()
            {
                return $"Action={Action.Method.Name} Subscriber={Subscriber} Type={MessageType.Name}";
            }

            public class SelectorWrapper<T>
            {
                public readonly Predicate<T> Selector;

                public SelectorWrapper(Predicate<T> selector)
                {
                    Selector = selector;
                }
            }
        }

        private readonly List<Handler> _handlers = new();

        internal void CheckHandlerCount()
        {
            var handlerCount = _handlers.Count;
            if (handlerCount == 0)
            {
                return;
            }
            foreach (var handler in _handlers)
            {
                Debug.Log($"handler {handler}");
            }
            Debug.LogWarning($"sceneUnloaded PubSubExtensions.HandlerCount is {handlerCount}");
        }

        public void Publish<T>(T data = default)
        {
            var handlerList = new List<Handler>();
            var handlersToRemoveList = new List<Handler>();

            // Ensure that we are in UNITY main thread.
            Assert.IsTrue(Time.frameCount >= 0);
            {
                foreach (var handler in _handlers)
                {
                    if (handler.Subscriber == null)
                    {
                        handlersToRemoveList.Add(handler);
                        continue;
                    }
                    if (handler.MessageType.IsAssignableFrom(typeof(T)))
                    {
                        handlerList.Add(handler);
                    }
                }

                foreach (var l in handlersToRemoveList)
                {
                    //-Debug.Log($"remove {l}");
                    _handlers.Remove(l);
                }
            }

            foreach (var handler in handlerList)
            {
                if (handler.Subscriber == null)
                {
                    continue;
                }
                if (!handler.Select(data))
                {
                    continue;
                }
                ((Action<T>)handler.Action)(data);
            }
        }

        public void Subscribe<T>(Object subscriber, Action<T> messageHandler, Predicate<T> messageSelector)
        {
            var selectorWrapper = messageSelector != null ? new Handler.SelectorWrapper<T>(messageSelector) : null;
            var item = new Handler(messageHandler, subscriber, typeof(T), selectorWrapper);
            // Ensure that we are in UNITY main thread.
            Assert.IsTrue(Time.frameCount >= 0);
            {
                //-Debug.Log($"subscribe {item}");
                _handlers.Add(item);
            }
        }

        public void Unsubscribe(Object subscriber)
        {
            // Ensure that we are in UNITY main thread.
            Assert.IsTrue(Time.frameCount >= 0);
            {
                var query = _handlers
                    .Where(handler => handler.Subscriber == null ||
                                      Equals(handler.Subscriber, subscriber));

                foreach (var h in query.ToList())
                {
                    //-Debug.Log($"unsubscribe {h}");
                    _handlers.Remove(h);
                }
            }
        }

        public void Unsubscribe<T>(Object subscriber, Action<T> handlerToRemove = null)
        {
            // Ensure that we are in UNITY main thread.
            Assert.IsTrue(Time.frameCount >= 0);
            {
                var query = _handlers
                    .Where(handler => handler.Subscriber == null ||
                                      (handler.MessageType == typeof(T) && Equals(handler.Subscriber, subscriber)));

                if (handlerToRemove != null)
                {
                    query = query.Where(handler => handler.Subscriber == null ||
                                                   handler.Action.Equals(handlerToRemove));
                }

                foreach (var h in query.ToList())
                {
                    //-Debug.Log($"unsubscribe {h}");
                    _handlers.Remove(h);
                }
            }
        }
    }
}