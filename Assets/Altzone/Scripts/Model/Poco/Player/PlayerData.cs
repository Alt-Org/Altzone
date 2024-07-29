using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Player
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(ClanData)), Optional] public string ClanId;
        [ForeignKey(nameof(CustomCharacter)), Optional] public int SelectedCharacterId;
        [ForeignKey(nameof(CustomCharacter)), Optional] public int[] SelectedCharacterIds = new int[5];
        [Unique] public string Name;

        public int DiamondSpeed = 100;
        public int DiamondResistance = 100;
        public int DiamondAttack = 100;
        public int DiamondDefence = 100;
        public int DiamondHP = 100;
        public int Eraser = 100;

        public int BackpackCapacity;

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        [Unique] public string UniqueIdentifier;

        public bool HasClanId => !string.IsNullOrEmpty(ClanId);

        public List<CustomCharacter> CustomCharacters { get; private set; }

        public BattleCharacter BattleCharacter => BattleCharacters.FirstOrDefault(x => x.CustomCharacterId == (CharacterID)SelectedCharacterIds[0]);
        public ReadOnlyCollection<BattleCharacter> BattleCharacters { get; private set; }

        public PlayerData(string id, string clanId, int currentCustomCharacterId, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(clanId.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(currentCustomCharacterId >= 0);
            Assert.IsTrue(name.IsMandatory());
            Assert.IsTrue(backpackCapacity >= 0);
            Assert.IsTrue(uniqueIdentifier.IsMandatory());
            Id = id;
            ClanId = clanId ?? string.Empty;
            SelectedCharacterId = currentCustomCharacterId;
            SelectedCharacterIds[0] = currentCustomCharacterId;
            Debug.Log("Test: "+SelectedCharacterIds[0]);
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
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(SelectedCharacterId)}: {SelectedCharacterId}," +
                $"{nameof(SelectedCharacterIds)}: {string.Join(",", SelectedCharacterIds)}, { nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}
