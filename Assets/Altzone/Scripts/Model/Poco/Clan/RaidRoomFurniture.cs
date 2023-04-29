using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoomFurniture
    {
        public string Id;
        public string GameFurnitureId;
        public int Row;
        public int Col;

        public RaidRoomFurniture(string id, string gameFurnitureId, int row, int col)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(gameFurnitureId));
            Assert.IsTrue(row >= 0);
            Assert.IsTrue(col >= 0);
            Id = id;
            GameFurnitureId = gameFurnitureId;
            Row = row;
            Col = col;
        }
    }
}