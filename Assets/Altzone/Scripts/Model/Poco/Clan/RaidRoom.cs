using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoom
    {
        public string Id;
        public int ClanMemberId;
        public RaidRoomType Type;
        public int RowCount;
        public int ColCount;
        public List<RaidRoomFurniture> Furniture = new();

        public RaidRoom(string id, int clanMemberId, RaidRoomType type, int rowCount, int colCount)
        {
            Id = id;
            ClanMemberId = clanMemberId;
            Type = type;
            RowCount = rowCount;
            ColCount = colCount;
        }
    }
}