using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoomFurniture
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(GameFurniture)), Mandatory] public string GameFurnitureId;
        public int Row;
        public int Col;

        public RaidRoomFurniture(string id, string gameFurnitureId, int row, int col)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(gameFurnitureId.IsMandatory());
            Assert.IsTrue(row >= 0);
            Assert.IsTrue(col >= 0);
            Id = id;
            GameFurnitureId = gameFurnitureId;
            Row = row;
            Col = col;
        }
    }
}