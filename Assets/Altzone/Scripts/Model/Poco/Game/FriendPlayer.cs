using System.Collections;
using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Game
{
    public class FriendPlayer
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string clan_id { get; set; }
        public AvatarData avatar { get; set; }

        public FriendPlayer(ServerFriendPlayer player)
        {
            _id = player._id;
            name = player.name;
            clan_id = player.clan_id;
            avatar = new(player.name, player.avatar);
        }
    }
}
