using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanData
    {
        public int Id;
        public string Name;
        public string Tag;
        public int GameCoins;

        public List<ClanMember> Members = new();
        public ClanInventory Inventory;
        public List<RaidRoom> Rooms;
    }
}