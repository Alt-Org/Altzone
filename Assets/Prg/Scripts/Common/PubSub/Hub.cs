using System;
using System.Collections.Generic;
using System.Linq;

namespace Prg.Scripts.Common.PubSub
{
    /// <summary>
    /// Simple Publish Subscribe Pattern <c>Hub</c> implementation using <c>WeakReference</c>.
    /// </summary>
    /// <remarks>
    /// This implementation is multi-threaded safe!
    /// </remarks>
    public class Hub
    {
        private class Handler
        {
            public readonly Delegate Action;
            public readonly WeakReference Subscriber;
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

            public Handler(Delegate action, WeakReference subscriber, Type messageType, object selectorWrapper)
            {
                Action = action;
                Subscriber = subscriber;
                MessageType = messageType;
                _selectorWrapper = selectorWrapper;
            }

            public override string ToString()
            {
                return $"Action={Action.Method.Name} Subscriber={(Subscriber.IsAlive ? Subscriber.Target : "_GC_")} Type={MessageType.Name}";
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

        private readonly object _locker = new();
        private readonly List<Handler> _handlers = new();

        public int CheckHandlerCount(bool isLogging = false)
        {
            int handlerCount;
            lock (_locker)
            {
                handlerCount = _handlers.Count;
                if (handlerCount == 0 || !isLogging)
                {
                    return handlerCount;
                }
                foreach (var handler in _handlers)
                {
                    Debug.Log($"handler {handler}");
                }
            }
            Debug.LogWarning($"handlerCount is {handlerCount}");
            return handlerCount;
        }

        public void Publish<T>(T data = default)
        {
            var handlerList = new List<Handler>();
            var handlersToRemoveList = new List<Handler>();

            lock (_locker)
            {
                foreach (var handler in _handlers)
                {
                    if (!handler.Subscriber.IsAlive)
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
                // Get reference to subscriber 1) to find that is is alive now and 2) to keep it alive during the callback.
                var target = handler.Subscriber.Target;
                if (target == null)
                {
                    continue;
                }
                if (!handler.Select(data))
                {
                    continue;
                }
                ((Action<T>)handler.Action)(data);

                // References the specified object, which makes it ineligible for garbage collection
                // from the start of the current routine to the point where this method is called.
                GC.KeepAlive(target);
            }
        }

        public void Subscribe<T>(object subscriber, Action<T> messageHandler, Predicate<T> messageSelector)
        {
            var selectorWrapper = messageSelector != null ? new Handler.SelectorWrapper<T>(messageSelector) : null;
            var item = new Handler(messageHandler, new WeakReference(subscriber), typeof(T), selectorWrapper);
            lock (_locker)
            {
                //-Debug.Log($"subscribe {item}");
                _handlers.Add(item);
            }
        }

        public void Unsubscribe(object subscriber)
        {
            lock (_locker)
            {
                var query = _handlers
                    .Where(handler => !handler.Subscriber.IsAlive ||
                                      Equals(handler.Subscriber.Target, subscriber));

                foreach (var h in query.ToList())
                {
                    //-Debug.Log($"unsubscribe {h}");
                    _handlers.Remove(h);
                }
            }
        }

        public void Unsubscribe<T>(object subscriber, Action<T> handlerToRemove = null)
        {
            lock (_locker)
            {
                var query = _handlers
                    .Where(handler => !handler.Subscriber.IsAlive ||
                                      (handler.MessageType == typeof(T) && Equals(handler.Subscriber.Target, subscriber)));

                if (handlerToRemove != null)
                {
                    query = query.Where(handler => !handler.Subscriber.IsAlive ||
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