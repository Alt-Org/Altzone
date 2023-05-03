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
        public int GameCoins;

        public ClanInventory Inventory = new();

        public List<ClanMember> Members = new();
        public List<RaidRoom> Rooms = new();

        public ClanData(string id, string name, string tag, int gameCoins)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            Assert.IsTrue(tag == null || !string.IsNullOrWhiteSpace(tag));
            Assert.IsTrue(gameCoins >= 0);
            Id = id;
            Name = name;
            Tag = tag;
            GameCoins = gameCoins;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Tag)}: {Tag}, {nameof(GameCoins)}: {GameCoins}" +
                   $", {nameof(Inventory)}: {Inventory}" +
                   $", {nameof(Members)}: {Members.Count}, {nameof(Rooms)}: {Rooms.Count}";
        }
    }
}