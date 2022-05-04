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
            public readonly Type Type;

            public Handler(Delegate action, WeakReference subscriber, Type type)
            {
                Action = action;
                Subscriber = subscriber;
                Type = type;
            }

            public override string ToString()
            {
                return $"A={Action.Method.Name}, S={(Subscriber.IsAlive ? Subscriber.Target : "dead")}, T={Type.Name}";
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
                        typeof(T) == h.Type)
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
                    else if (handler.Type.IsAssignableFrom(typeof(T)))
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
                ((Action<T>)l.Action)(data);
            }
        }

        /// <summary>
        /// Allow subscribing directly to this Hub.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void Subscribe<T>(Action<T> handler)
        {
            Subscribe(this, handler);
        }

        public void Subscribe<T>(object subscriber, Action<T> handler)
        {
            var item = new Handler(handler, new WeakReference(subscriber), typeof(T));
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
                                (a.Subscriber.Target.Equals(subscriber) && a.Type == typeof(T)));

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