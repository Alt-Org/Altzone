using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Game
{
    public enum FriendRequestDirection
    {
        None,
        Outgoing,
        Incoming
    }

    public class FriendRequest 
    {
        public string friendship_id { get; set; }
        public FriendRequestDirection direction { get; set; }
        public FriendPlayer friend { get; set; }
        public string clan_id { get; set; }
        public string clanName { get; set; }

        public FriendRequest(ServerFriendRequest request)
        {
            friendship_id = request.friendship_id;
            if(request.direction == "incoming") { direction = FriendRequestDirection.Incoming; }
            else if(request.direction == "outgoing") { direction = FriendRequestDirection.Outgoing; }
            friend = new(request.friend);
            clanName = request.clanName;
            clan_id = request.clan_id;
        }
    }
}
