using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanFurniture
    {
        public string Id;
        public string GameFurnitureId;

        public ClanFurniture(string id, string gameFurnitureId)
        {
            Id = id;
            GameFurnitureId = gameFurnitureId;
        }
    }
}