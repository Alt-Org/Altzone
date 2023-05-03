using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Player
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(ClanData)), Optional] public string ClanId;
        [ForeignKey(nameof(CustomCharacter)), Optional] public string CurrentCustomCharacterId;
        [Unique] public string Name;
        public int BackpackCapacity;

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        [Mandatory] public string UniqueIdentifier;

        public bool HasClanId => !string.IsNullOrEmpty(ClanId);

        public List<CustomCharacter> CustomCharacters { get; private set; }

        public BattleCharacter BattleCharacter => BattleCharacters.FirstOrDefault(x => x.CustomCharacterId == CurrentCustomCharacterId);
        public ReadOnlyCollection<BattleCharacter> BattleCharacters { get; private set; }

        public PlayerData(string id, string clanId, string currentCustomCharacterId, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(id));
            Assert.IsTrue(clanId == null || !string.IsNullOrWhiteSpace(clanId));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(currentCustomCharacterId));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(name));
            Assert.IsTrue(backpackCapacity >= 0);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(uniqueIdentifier));
            Id = id;
            ClanId = clanId;
            CurrentCustomCharacterId = currentCustomCharacterId;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        internal void Patch(List<BattleCharacter> battleCharacters, List<CustomCharacter> customCharacters)
        {
            BattleCharacters = new ReadOnlyCollection<BattleCharacter>(battleCharacters);
            CustomCharacters = new ReadOnlyCollection<CustomCharacter>(customCharacters).ToList();
        }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(CurrentCustomCharacterId)}: {CurrentCustomCharacterId}" +
                $", {nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}