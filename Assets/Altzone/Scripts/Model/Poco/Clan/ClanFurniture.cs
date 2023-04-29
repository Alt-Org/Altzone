using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanFurniture
    {
        public string Id;
        public string GameFurnitureId;

        public ClanFurniture(string id, string gameFurnitureId)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(gameFurnitureId));
            Id = id;
            GameFurnitureId = gameFurnitureId;
        }
    }
}