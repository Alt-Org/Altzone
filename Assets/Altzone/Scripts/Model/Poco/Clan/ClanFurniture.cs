using System;
using System.Diagnostics.CodeAnalysis;
using Prg.Scripts.Common.Extensions;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanFurniture
    {
        public string Id;
        public string GameFurnitureName;
        public Vector2Int Position = new(-1, -1);
        public FurnitureGrid FurnitureGrid = FurnitureGrid.None;
        public int Room = -1;
        public bool IsRotated;
        public bool InVoting = false;
        public bool VotedToSell = false;

        public ClanFurniture(string id, string gameFurnitureId, int column = -1, int row = -1, FurnitureGrid grid = FurnitureGrid.None, int room = -1, bool isRotated = false)
        {
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(gameFurnitureId.IsSet());
            Id = id;
            GameFurnitureName = gameFurnitureId;
            Position = new(column, row);
            FurnitureGrid = grid;
            Room = room;
            IsRotated = isRotated;
        }
    }
}
