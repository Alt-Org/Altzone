using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class FriendPlayer
    {
        public string friendshipId { get; set; }
        public string _id { get; set; }
        public string name { get; set; }
        public string clan_id { get; set; }
        public AvatarData avatar { get; set; }

        public FriendPlayer() { }

        public FriendPlayer(ServerFriendPlayer player)
        {
            friendshipId = player._friendship_id;
            _id = player._id;
            name = player.name;
            clan_id = player.clan_id;
            if(player.avatar != null)avatar = new(player.name, player.avatar);
        }
    }
}
