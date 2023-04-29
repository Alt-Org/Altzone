using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoom
    {
        public string Id;
        public string ClanMemberId;
        public RaidRoomType Type;
        public int RowCount;
        public int ColCount;
        public List<RaidRoomFurniture> Furniture = new();

        public RaidRoom(string id, string clanMemberId, RaidRoomType type, int rowCount, int colCount)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(clanMemberId));
            Id = id;
            ClanMemberId = clanMemberId;
            Type = type;
            RowCount = rowCount;
            ColCount = colCount;
        }
    }
}