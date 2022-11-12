using System;
using System.Collections.Generic;
using System.Linq;

namespace Prg.Scripts.Common.PubSub
{
    public class Hub
    {
        internal class Handler
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
                return $"A={Action.Method.Name}, S={(Subscriber.IsAlive ? Subscriber.Target : "dead")}, T={MessageType.Name}";
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

        private readonly object _locker = new object();
        internal readonly List<Handler> Handlers = new List<Handler>();

        public bool Exists<T>(object subscriber)
        {
            lock (_locker)
            {
                foreach (var h in Handlers)
                {
                    if (h.Subscriber.IsAlive &&
                        Equals(h.Subscriber.Target, subscriber) &&
                        typeof(T) == h.MessageType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Allow publishing directly onto this Hub.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void Publish<T>(T data = default)
        {
            var handlerList = new List<Handler>();
            var handlersToRemoveList = new List<Handler>();

            lock (_locker)
            {
                foreach (var handler in Handlers)
                {
                    if (!handler.Subscriber.IsAlive)
                    {
                        handlersToRemoveList.Add(handler);
                    }
                    else if (handler.MessageType.IsAssignableFrom(typeof(T)))
                    {
                        handlerList.Add(handler);
                    }
                }

                foreach (var l in handlersToRemoveList)
                {
                    //-Debug.Log($"remove {l}");
                    Handlers.Remove(l);
                }
            }

            foreach (var l in handlerList)
            {
                if (l.Select(data))
                {
                    ((Action<T>)l.Action)(data);
                }
            }
        }

        /// <summary>
        /// Allow subscribing directly to this Hub.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageHandler"></param>
        /// <param name="messageSelector"></param>
        public void Subscribe<T>(Action<T> messageHandler, Predicate<T> messageSelector)
        {
            Subscribe(this, messageHandler, messageSelector);
        }

        public void Subscribe<T>(object subscriber, Action<T> messageHandler, Predicate<T> messageSelector)
        {
            var selectorWrapper = messageSelector != null ? new Handler.SelectorWrapper<T>(messageSelector) : null;
            var item = new Handler(messageHandler, new WeakReference(subscriber), typeof(T), selectorWrapper);
            lock (_locker)
            {
                //-Debug.Log($"subscribe {item}");
                Handlers.Add(item);
            }
        }

        /// <summary>
        /// Allow unsubscribing directly to this Hub.
        /// </summary>
        public void Unsubscribe()
        {
            Unsubscribe(this);
        }

        public void Unsubscribe(object subscriber)
        {
            lock (_locker)
            {
                var query = Handlers.Where(a => !a.Subscriber.IsAlive ||
                                                a.Subscriber.Target.Equals(subscriber));

                foreach (var h in query.ToList())
                {
                    //-Debug.Log($"unsubscribe {h}");
                    Handlers.Remove(h);
                }
            }
        }

        /// <summary>
        /// Allow unsubscribing directly to this Hub.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>()
        {
            Unsubscribe<T>(this);
        }

        /// <summary>
        /// Allow unsubscribing directly to this Hub.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void Unsubscribe<T>(Action<T> handler)
        {
            Unsubscribe(this, handler);
        }

        public void Unsubscribe<T>(object subscriber, Action<T> handler = null)
        {
            lock (_locker)
            {
                var query = Handlers
                    .Where(a => !a.Subscriber.IsAlive ||
                                (a.Subscriber.Target.Equals(subscriber) && a.MessageType == typeof(T)));

                if (handler != null)
                {
                    query = query.Where(a => a.Action.Equals(handler));
                }

                foreach (var h in query.ToList())
                {
                    //-Debug.Log($"unsubscribe {h}");
                    Handlers.Remove(h);
                }
            }
        }
    }
}