using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanFurniture
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(GameFurniture)), Mandatory] public string GameFurnitureName;
        public Vector2Int Position = new(-1, -1);
        public FurnitureGrid FurnitureGrid = FurnitureGrid.None;
        public int Room = -1;
        public bool IsRotated;

        public ClanFurniture(string id, string gameFurnitureId, int column = -1, int row = -1, FurnitureGrid grid = FurnitureGrid.None, int room = -1, bool isRotated = false)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(gameFurnitureId.IsMandatory());
            Id = id;
            GameFurnitureName = gameFurnitureId;
            Position = new(column, row);
            FurnitureGrid = grid;
            Room = room;
            IsRotated = isRotated;
        }
    }
}
