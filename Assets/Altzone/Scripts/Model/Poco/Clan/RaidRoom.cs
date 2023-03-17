using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoom
    {
        public int Id;
        public int ClanMemberId;
        public string GameFurnitureId;
        public RaidRoomType Type;

        public RaidRoom(int id, int clanMemberId, string gameFurnitureId, RaidRoomType type)
        {
            Id = id;
            ClanMemberId = clanMemberId;
            GameFurnitureId = gameFurnitureId;
            Type = type;
        }
    }
}