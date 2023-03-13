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

        public ClanInventory Inventory = new();

        public List<ClanMember> Members = new();
        public List<RaidRoom> Rooms = new();

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Tag)}: {Tag}, {nameof(GameCoins)}: {GameCoins}" +
                   $", {nameof(Inventory)}: {Inventory}" +
                   $", {nameof(Members)}: {Members.Count}, {nameof(Rooms)}: {Rooms.Count}";
        }
    }
}