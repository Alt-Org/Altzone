using System;
using System.Diagnostics.CodeAnalysis;
using Prg.Scripts.Common.Extensions;
using Altzone.Scripts.Model.Poco.Game;
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
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(gameFurnitureId.IsSet());
            Assert.IsTrue(row >= 0);
            Assert.IsTrue(col >= 0);
            Id = id;
            GameFurnitureId = gameFurnitureId;
            Row = row;
            Col = col;
        }
    }
}
