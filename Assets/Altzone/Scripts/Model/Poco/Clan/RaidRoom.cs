using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoom
    {
        public int Id;
        public int ClanMemberId;
        public RaidRoomType Type;
        public List<RaidRoomFurniture> Furniture = new();

        public RaidRoom(int id, int clanMemberId, RaidRoomType type)
        {
            Id = id;
            ClanMemberId = clanMemberId;
            Type = type;
        }
    }
}