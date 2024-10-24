using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanData
    {
        [PrimaryKey] public string Id;
        [Unique] public string Name;
        [Optional] public string Tag;
        [Optional] public string Phrase;
        public int GameCoins;

        public List<string> Labels = new();
        public ClanRoleRights[] ClanRights;

        public ClanInventory Inventory = new();

        public List<ClanMember> Members = new();
        public List<RaidRoom> Rooms = new();

        public ClanAge ClanAge;
        public Language Language;
        public Goals Goals;

        public ClanData(string id, string name, string tag, int gameCoins)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(name.IsMandatory());
            Assert.IsTrue(tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(gameCoins >= 0);
            Id = id;
            Name = name;
            Tag = tag ?? string.Empty;
            GameCoins = gameCoins;
        }

        public ClanData(ServerClan clan)
        {
            Assert.IsTrue(clan._id.IsPrimaryKey());
            Assert.IsTrue(clan.name.IsMandatory());
            Assert.IsTrue(clan.tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(clan.gameCoins >= 0);
            Id = clan._id;
            Name = clan.name;
            Tag = clan.tag ?? string.Empty;
            Phrase = clan.phrase ?? string.Empty;
            GameCoins = clan.gameCoins;
            Labels = clan.labels;
            ClanAge = clan.clanAge;
            Language = clan.language;
            Goals = clan.goals;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Tag)}: {Tag}, {nameof(GameCoins)}: {GameCoins}" +
                   $", {nameof(Inventory)}: {Inventory}" +
                   $", {nameof(Members)}: {Members.Count}, {nameof(Rooms)}: {Rooms.Count}";
        }
    }
}
