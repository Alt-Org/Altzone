using System;
using System.Collections.Generic;
using System.Linq;

namespace Prg.Scripts.Common.PubSub
{
    /// <summary>
    /// Simple Publish Subscribe Pattern <c>Hub</c> implementation.
    /// </summary>
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
        internal readonly List<Handler> Handlers = new();

        /// <summary>
        /// Checks if subscriber has subscribed to given message (type).
        /// </summary>
        /// <param name="subscriber"></param>
        /// <typeparam name="T"></typeparam>
        public bool Exists<T>(object subscriber)
        {
            lock (_locker)
            {
                foreach (var handler in Handlers)
                {
                    if (!handler.Subscriber.IsAlive)
                    {
                        // This is actually not needed but used to emphasize the fact that we are using a WeakReference here:
                        // - h.Subscriber.Target will be null if it has been Garbage Collected and Equals() test below fails.
                        continue;
                    }
                    if (handler.MessageType == typeof(T) && Equals(handler.Subscriber.Target, subscriber))
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
                    Handlers.Remove(l);
                }
            }

            foreach (var handler in handlerList)
            {
                if (!handler.Select(data))
                {
                    continue;
                }
                // Get reference to subscriber 1) to find that is is alive now and 2) to keep it alive during the callback.
                // Note that this does not apply UnityEngine.Object's because they are managed in C++ side,
                // but null check works fine for them as well.
                var target = handler.Subscriber.Target;
                if (target == null)
                {
                    continue;
                }
                ((Action<T>)handler.Action)(data);
                // References the specified object, which makes it ineligible for garbage collection
                // from the start of the current routine to the point where this method is called.
                GC.KeepAlive(target);
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
                var query = Handlers.Where(handler => !handler.Subscriber.IsAlive ||
                                                      Equals(handler.Subscriber.Target, subscriber));

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

        public void Unsubscribe<T>(object subscriber, Action<T> handlerToRemove = null)
        {
            lock (_locker)
            {
                var query = Handlers
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
                    Handlers.Remove(h);
                }
            }
        }
    }
}