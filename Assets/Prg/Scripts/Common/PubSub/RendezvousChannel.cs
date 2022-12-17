using System;
using System.Collections.Generic;
using System.Linq;

namespace Prg.Scripts.Common.PubSub
{
    /// <summary>
    /// Container for Rendezvous.IChannel interface to keep its name un-ambiguous.
    /// </summary>
    public static class Rendezvous
    {
        /// <summary>
        /// Rendezvous channel instance "handle"
        /// </summary>
        public interface IChannel
        {
            string channel { get; }
            object instance { get; }

            void onJoinChannel(IChannel id);
            void onLeaveChannel(IChannel id);
        }

        /// <summary>
        /// Convenience method to create channel instance handles using default implementation.
        /// </summary>
        public static IChannel CreateChannel(string channel, object instance, Action<IChannel> onJoin = null,
            Action<IChannel> onLeave = null)
        {
            return new _RendezvousChannel(channel, instance, onJoin, onLeave);
        }

        /// <summary>
        /// Convenience method to leave channel.
        /// </summary>
        public static void LeaveChannel(IChannel channel)
        {
            channel.RendezvousChannelLeave();
        }

        /// <summary>
        /// Default Rendezvous.IChannel interface implementation.
        /// </summary>
        private class _RendezvousChannel : IChannel
        {
            public string channel { get; }
            public object instance { get; }

            private readonly Action<IChannel> onJoin;
            private readonly Action<IChannel> onLeave;

            public _RendezvousChannel(string channel, object instance, Action<IChannel> onJoin = null, Action<IChannel> onLeave = null)
            {
                this.channel = channel;
                this.instance = instance;
                this.onJoin = onJoin;
                this.onLeave = onLeave;
                Debug.Log($"RendezvousChannelJoin {this}");
                this.RendezvousChannelJoin();
            }

            public void Leave()
            {
                Debug.Log($"RendezvousChannelLeave {this}");
                this.RendezvousChannelLeave();
            }

            public void onJoinChannel(IChannel id)
            {
                Debug.Log($"onJoinChannel {this} <++ {id}");
                onJoin?.Invoke(id);
            }

            public void onLeaveChannel(IChannel id)
            {
                Debug.Log($"onLeaveChannel {this} <~~ {id}");
                onLeave?.Invoke(id);
            }

            public override string ToString()
            {
                return $"{channel}:{instance.GetType().Name}:{instance.GetHashCode():X}";
            }
        }
    }

    public class RendezvousChannel
    {
        private readonly HashSet<ChannelBinding> clients = new HashSet<ChannelBinding>();

        public void RendezvousChannelJoin(Rendezvous.IChannel client)
        {
            // Introduce new client to current clients
            var newBinding = new ChannelBinding(client);
            foreach (var currentBinding in clients)
            {
                if (newBinding.host.channel.Equals(currentBinding.host.channel, StringComparison.Ordinal))
                {
                    currentBinding.introduce(newBinding);
                    newBinding.introduce(currentBinding);
                }
            }
            if (!clients.Add(newBinding))
            {
                Debug.LogWarning($"RendezvousChannelJoin {client} already exists");
            }
        }

        public void RendezvousChannelLeave(Rendezvous.IChannel client)
        {
            var oldBinding = clients.FirstOrDefault(x => x.host.Equals(client));
            if (oldBinding == null)
            {
                Debug.LogWarning($"RendezvousChannelLeave {client} not found");
                return;
            }
            clients.Remove(oldBinding);
            // Un-introduce old client from remaining (current) clients
            foreach (var currentClient in clients)
            {
                if (oldBinding.host.channel.Equals(currentClient.host.channel, StringComparison.Ordinal))
                {
                    oldBinding.unIntroduce(currentClient);
                    currentClient.unIntroduce(oldBinding);
                }
            }
        }

        private class ChannelBinding
        {
            public readonly Rendezvous.IChannel host;
            private readonly HashSet<Rendezvous.IChannel> friends = new HashSet<Rendezvous.IChannel>();

            public ChannelBinding(Rendezvous.IChannel host)
            {
                this.host = host;
            }

            public void introduce(ChannelBinding other)
            {
                if (friends.Add(other.host))
                {
                    other.host.onJoinChannel(this.host);
                }
            }

            public void unIntroduce(ChannelBinding other)
            {
                if (friends.Remove(other.host))
                {
                    other.host.onLeaveChannel(this.host);
                }
            }

            private bool Equals(ChannelBinding other)
            {
                return Equals(host, other.host);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((ChannelBinding)obj);
            }

            public override int GetHashCode()
            {
                return host.channel.GetHashCode() * host.instance.GetHashCode();
            }
        }
    }
}