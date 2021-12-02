using System;
using System.Collections.Generic;
using System.Linq;

namespace Prg.Scripts.Common.PubSub
{
    public class RendezvousHub
    {
        private class Handler
        {
            public Delegate Action { get; set; }
            public WeakReference Client { get; set; }
        }

        private readonly Dictionary<Type, Handler> pairHandlers = new Dictionary<Type, Handler>();
        private readonly Dictionary<Type, List<Handler>> listHandlers = new Dictionary<Type, List<Handler>>();

        public void Rendezvous<T>(T client, Action<T> handshake)
        {
            var type = typeof(T);
            if (pairHandlers.TryGetValue(type, out var handler) && handler.Client.IsAlive)
            {
                pairHandlers.Remove(type);
                //Debug.Log($"Rendezvous {client} with {handler.Client.Target}");
                handshake((T) handler.Client.Target);
                ((Action<T>) handler.Action)(client);
                return;
            }
            //Debug.Log($"Rendezvous {client} wait for the other client");
            handler = new Handler
            {
                Action = handshake,
                Client = new WeakReference(client)
            };
            pairHandlers.Add(type, handler);
        }

        public void Rendezvous<T>(T client, Action<T> handshake, Predicate<T> selector)
        {
            //Debug.Log($"Rendezvous {client} start");
            var type = typeof(T);
            if (listHandlers.TryGetValue(type, out var handlerList))
            {
                var actionList = new List<Handler>();
                var query = handlerList.Where(x => x.Client.IsAlive && !client.Equals(x.Client.Target));
                foreach (var handlerCandidate in query)
                {
                    if (selector((T) handlerCandidate.Client.Target))
                    {
                        // Handler must be copied to separate list because they may update original handler list, eg. remove themself
                        actionList.Add(handlerCandidate);
                    }
                }
                if (actionList.Count > 0)
                {
                    //var actionCounter = 0;
                    foreach (var selectedHandler in actionList)
                    {
                        //Debug.Log($"Rendezvous #{++actionCounter} {client} with {selectedHandler.Client.Target}");
                        handshake((T) selectedHandler.Client.Target);
                        ((Action<T>) selectedHandler.Action)(client);
                    }
                    return;
                }
            }
            //Debug.Log($"Rendezvous {client} wait for other client(s) on list {handlerList?.Count ?? 0}");
            if (handlerList == null)
            {
                handlerList = new List<Handler>();
                listHandlers.Add(type, handlerList);
            }
            var handler = new Handler
            {
                Action = handshake,
                Client = new WeakReference(client)
            };
            handlerList.Add(handler);
        }

        public void CancelRendezvous<T>(T client, Action<T> handshake)
        {
            var type = typeof(T);
            if (pairHandlers.Remove(type))
            {
                //Debug.Log($"CancelRendezvous {client} single");
            }
            if (listHandlers.TryGetValue(type, out var handlerList))
            {
                var eligibleHandlers = handlerList
                    .Where(x => !x.Client.IsAlive || client.Equals(x.Client.Target) && handshake.Equals(x.Action))
                    .ToList();
                if (eligibleHandlers.Count > 0)
                {
                    foreach (var handler in eligibleHandlers)
                    {
                        handlerList.Remove(handler);
                    }
                    //Debug.Log($"CancelRendezvous {client} from list {handlerList.Count}");
                }
            }
        }
    }
}