using System;

namespace Prg.Scripts.Common.PubSub
{
    internal static class RendezvousExtensions
    {
        private static readonly RendezvousHub hub = new RendezvousHub();
        private static readonly RendezvousChannel channel = new RendezvousChannel();

        #region Rendezvous for objects of exactly same type

        public static void Rendezvous<T>(this T client, Action<T> handshake)
        {
            hub.Rendezvous(client, handshake);
        }

        public static void Rendezvous<T>(this T client, Action<T> handshake, Predicate<T> selector)
        {
            hub.Rendezvous(client, handshake, selector);
        }

        public static void CancelRendezvous<T>(this T client, Action<T> handshake)
        {
            hub.CancelRendezvous(client, handshake);
        }

        #endregion

        #region Rendezvous for "channel" of objects of any type

        public static void RendezvousChannelJoin(this Rendezvous.IChannel client)
        {
            channel.RendezvousChannelJoin(client);
        }

        public static void RendezvousChannelLeave(this Rendezvous.IChannel client)
        {
            channel.RendezvousChannelLeave(client);
        }

        #endregion
    }
}