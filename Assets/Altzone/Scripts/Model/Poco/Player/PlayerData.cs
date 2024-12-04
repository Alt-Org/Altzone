using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
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

        private List<CustomCharacter> _characterList;
        private List<BattleCharacter> _battleCharacters;

        public int DiamondSpeed = 1000;
        public int DiamondResistance = 1000;
        public int DiamondAttack = 1000;
        public int DiamondDefence = 1000;
        public int DiamondHP = 1000;
        public int Eraser = 1000;

        public int BackpackCapacity;

        public int dailyTaskId = 0;

        public int points = 0;

        public List<PlayerVoteData> playerVotes = new List<PlayerVoteData>();

        public ServerGameStatistics stats = null;
        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        [Unique] public string UniqueIdentifier;

        public bool HasClanId => !string.IsNullOrEmpty(ClanId);

        public List<CustomCharacter> CustomCharacters { get; private set; }

        public BattleCharacter BattleCharacter => BattleCharacters.FirstOrDefault(x => x.CharacterID == (CharacterID)SelectedCharacterIds[0]);
        public ReadOnlyCollection<BattleCharacter> BattleCharacters { get; private set; }

        public ReadOnlyCollection<CustomCharacter> CurrentBattleCharacters
        {
            get
            {
                List<CustomCharacter> list = new();
                foreach (var id in SelectedCharacterIds)
                {
                    if (id == 0) continue;
                    list.Add(CustomCharacters.FirstOrDefault(x => x.Id == (CharacterID)id));
                }
                while(list.Count < 5)
                {
                    list.Add(CustomCharacter.CreateEmpty());
                }
                return new ReadOnlyCollection<CustomCharacter>(list);
            }

        }

        public PlayerData(string id, string clanId, int currentCustomCharacterId, int[]currentBattleCharacterIds, string name, int backpackCapacity, string uniqueIdentifier)
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
            SelectedCharacterIds = currentBattleCharacterIds;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        public PlayerData(ServerPlayer player)
        {
            Assert.IsTrue(player._id.IsPrimaryKey());
            Assert.IsTrue(player.clan_id.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(player.currentCustomCharacterId >= 0);
            Assert.IsTrue(player.name.IsMandatory());
            Assert.IsTrue(player.backpackCapacity >= 0);
            Assert.IsTrue(player.uniqueIdentifier.IsMandatory());
            Id = player._id;
            ClanId = player.clan_id ?? string.Empty;
            SelectedCharacterId = 0;
            SelectedCharacterIds = null;
            Name = player.name;
            BackpackCapacity = player.backpackCapacity;
            UniqueIdentifier = player.uniqueIdentifier;
            points = player.points;
            stats = player.gameStatistics;
        }

        public void UpdateCustomCharacter(CustomCharacter character)
        {
            if (character == null) return;
            if (character.Speed != 0)
            {
                Debug.LogWarning($"Speed has been modified. Setting to 0.");
                character.Speed = 0;
            }
            int statCheck = 0;
            statCheck += character.Hp;
            statCheck += character.Attack;
            statCheck += character.Defence;
            statCheck += character.Resistance;
            if(statCheck >= 100)
            {
                Debug.LogError($"Invalid total stat increases: {statCheck}, too high.");
                return;
            }

            int i = 0;
            foreach (CustomCharacter item in _characterList)
            {
                if (item.Id != character.Id)
                {
                    i++;
                    continue;
                }

                _characterList[i] = character;
                break;
            }
            Patch(); // This possible gets fired too often.
        }

        internal void BuildCharacterLists(List<BattleCharacter> battleCharacters, List<CustomCharacter> customCharacters)
        {
            _battleCharacters = battleCharacters;
            _characterList = customCharacters;
            Patch(_battleCharacters, _characterList);
        }

        internal void Patch()
        {
            Patch(GetAllBattleCharacters(), _characterList);
        }

        internal void Patch(List<BattleCharacter> battleCharacters, List<CustomCharacter> customCharacters)
        {
            BattleCharacters = new ReadOnlyCollection<BattleCharacter>(battleCharacters);
            CustomCharacters = new ReadOnlyCollection<CustomCharacter>(customCharacters).ToList();
        }

        private List<BattleCharacter> GetAllBattleCharacters()
        {
            var battleCharacters = new List<BattleCharacter>();
            foreach (var customCharacter in _characterList)
            {
                battleCharacters.Add(GetBattleCharacter(customCharacter.Id));
            }
            return battleCharacters;
        }

        private BattleCharacter GetBattleCharacter(CharacterID customCharacterId)
        {
            var customCharacter = _characterList.FirstOrDefault(x => x.Id == customCharacterId);
            if (customCharacter == null)
            {
                throw new UnityException($"CustomCharacter not found for {customCharacterId}");
            }
            ReadOnlyCollection<CharacterClass> characterClasses = null;
            Storefront.Get().GetAllCharacterClassesYield(result => characterClasses = result);
            if(characterClasses == null)
            {
                Debug.LogError($"Unable to fetch characterClasses.");
                return null;
            }
            var characterClass =
                characterClasses.FirstOrDefault(x => x.Id == customCharacter.CharacterClassID);
            if (characterClass == null)
            {
                Debug.LogError($"Unable to find characterClass.");
                return null;
            }
            return BattleCharacter.Create(customCharacter, characterClass);
        }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(SelectedCharacterId)}: {SelectedCharacterId}," +
                $"{nameof(SelectedCharacterIds)}: {string.Join(",", SelectedCharacterIds)}, { nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}
