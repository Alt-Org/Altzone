using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanInventory
    {
        public List<ClanFurniture> Furniture = new();

        public override string ToString()
        {
            return $"{nameof(Furniture)}: {Furniture.Count}";
        }
    }
}