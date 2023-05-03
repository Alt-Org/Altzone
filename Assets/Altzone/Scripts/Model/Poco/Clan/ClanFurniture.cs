using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanFurniture
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(GameFurniture)), Mandatory] public string GameFurnitureId;

        public ClanFurniture(string id, string gameFurnitureId)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(gameFurnitureId.IsMandatory());
            Id = id;
            GameFurnitureId = gameFurnitureId;
        }
    }
}