using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2.Internal;
using Altzone.Scripts.Voting;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Player
{

    public enum PlayStyles
    {
        Harjoittelja,
        Intohimoinen,
        Sisustaja,
        Kilpapelaaja,
        Kasuaalipelaaja,
        Sosiaalinen,
        Taukopelaaja,
        Grindaaja,
        Saavutusten_Metsastaja,
        Tarkka_Strategikko,
        Fiilistelija,
        Ongelmanratkaisija,
        Huono_Haviaja
    };

    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(ClanData)), Optional] public string ClanId;
        [ForeignKey(nameof(CustomCharacter)), Optional] public int SelectedCharacterId;
        [ForeignKey(nameof(CustomCharacter)), Optional] public string[] SelectedCharacterIds = new string[3] { ((int)CharacterID.None).ToString(), ((int)CharacterID.None).ToString(), ((int)CharacterID.None).ToString() };
        [ForeignKey(nameof(CustomCharacter)), Optional] public int[] SelectedTestCharacterIds;
        [Unique] public string Name;

        private List<CustomCharacter> _characterList;
        public List<CustomCharacter> TestCharacterList;

        public int DiamondSpeed = 1000;
        public int DiamondCharacterSize = 1000;
        public int DiamondAttack = 1000;
        public int DiamondDefence = 1000;
        public int DiamondHP = 1000;
        public int Eraser = 1000;

        public int BackpackCapacity;

        public PlayerTask Task = null;
        public AvatarData AvatarData;

        public int points = 0;

        public PlayStyles playStyles;

       public string emotionSelectorDate = null;

        public string daysBetweenInput = "0";

        public List<string> _playerDataEmotionList = new List<string> { Emotion.Blank.ToString(), Emotion.Love.ToString(), Emotion.Playful.ToString(), Emotion.Joy.ToString(), Emotion.Sorrow.ToString(), Emotion.Anger.ToString(), Emotion.Blank.ToString() };

        public List<PlayerVoteData> playerVotes = new List<PlayerVoteData>();

        public ServerGameStatistics stats = null;

        public string ChosenMotto = null;
        public string FavoriteDefenceID = null;

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        [Unique] public string UniqueIdentifier;

        public bool HasClanId => !string.IsNullOrEmpty(ClanId);

        public List<CustomCharacter> CustomCharacters { get; private set; }

        public ReadOnlyCollection<CustomCharacter> CurrentBattleCharacters
        {
            get
            {
                List<CustomCharacter> list = new();
                for (int i = 0; i < SelectedCharacterIds.Length; i++)
                {
                    string serverId = SelectedCharacterIds[i];
                    CharacterID testCharId = SelectedTestCharacterIds == null ? CharacterID.None : (CharacterID)SelectedTestCharacterIds[i];
                    bool isTestCharacter = testCharId != CharacterID.None;

                    if (string.IsNullOrEmpty(serverId) && !isTestCharacter)
                    {
                        list.Add(CustomCharacter.CreateEmpty());
                        continue;
                    }

                    CustomCharacter character = isTestCharacter ? CustomCharacters.FirstOrDefault(x => x.Id == testCharId) : CustomCharacters.FirstOrDefault(x => x.ServerID == serverId);
                    if(character == null) continue;
                    list.Add(character);
                }
                return new ReadOnlyCollection<CustomCharacter>(list);
            }
        }

        public List<Emotion> playerDataEmotionList
        {
            get
            {
                List<Emotion> list = new();
                foreach(string emotion in _playerDataEmotionList)
                {
                    list.Add((Emotion)Enum.Parse(typeof(Emotion), emotion));
                }
                return list;
            }
            set
            {
                List<string> list = new();
                foreach (Emotion emotion in value)
                {
                    list.Add(emotion.ToString());
                }
                _playerDataEmotionList = list;
            }
        }

        public PlayerData(string id, string clanId, int currentCustomCharacterId, string[]currentBattleCharacterIds, int[] currentTestCharacterIds, string name, int backpackCapacity, string uniqueIdentifier)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(clanId.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(currentCustomCharacterId >= 0);
            Assert.IsTrue(name.IsMandatory());
            Assert.IsTrue(backpackCapacity >= 0);
            Assert.IsTrue(uniqueIdentifier.IsMandatory());
            Id = id;
            ClanId = clanId ?? string.Empty;
            SelectedCharacterId = currentCustomCharacterId;
            SelectedCharacterIds = currentBattleCharacterIds;
            if (currentTestCharacterIds == null) SetTestCharacterIds();
            else SelectedTestCharacterIds = currentTestCharacterIds;
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        public PlayerData(ServerPlayer player, bool limited = false)
        {
            Assert.IsTrue(player._id.IsPrimaryKey());
            Assert.IsTrue(player.clan_id.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(player.currentCustomCharacterId >= 0);
            Assert.IsTrue(player.name.IsMandatory());
            if (!limited) Assert.IsTrue(player.backpackCapacity >= 0);
            Assert.IsTrue(player.uniqueIdentifier.IsMandatory());
            Id = player._id;
            ClanId = player.clan_id ?? string.Empty;
            SelectedCharacterId = (int)(player.currentAvatarId == null ? 0 : player.currentAvatarId);
            string noCharacter = ((int)CharacterID.None).ToString();
            if (!limited)SelectedCharacterIds = (player?.battleCharacter_ids == null || player.battleCharacter_ids.Length < 3) ? new string[3] { noCharacter, noCharacter, noCharacter } : player.battleCharacter_ids;
            Name = player.name;
            if (!limited) BackpackCapacity = player.backpackCapacity;
            UniqueIdentifier = player.uniqueIdentifier;
            points = player.points;
            stats = player.gameStatistics;
            Task = player.DailyTask != null ? new(player.DailyTask) : null;
            AvatarData = player.avatar != null ? new(player.name, player.avatar) : null;
            if (!limited) Task = player.DailyTask != null ? new(player.DailyTask): null;

            if (CharacterSpecConfig.Instance.AllowTestCharacters) SetTestCharacterIds();
        }

        public void UpdatePlayerData(ServerPlayer player)
        {
            Assert.IsTrue(player._id.IsPrimaryKey());
            Assert.IsTrue(player.clan_id.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(player.currentCustomCharacterId >= 0);
            Assert.IsTrue(player.name.IsMandatory());
            Assert.IsTrue(player.backpackCapacity >= 0);
            Assert.IsTrue(player.uniqueIdentifier.IsMandatory());
            Id = player._id;
            ClanId = player.clan_id ?? string.Empty;
            SelectedCharacterId = (int)(player.currentAvatarId == null ? 0 : player.currentAvatarId);
            string noCharacter = ((int)CharacterID.None).ToString();
            SelectedCharacterIds = player?.battleCharacter_ids == null || player.battleCharacter_ids.Length < 3 ? new string[3] { noCharacter, noCharacter, noCharacter } : player.battleCharacter_ids;
            Name = player.name;
            BackpackCapacity = player.backpackCapacity;
            UniqueIdentifier = player.uniqueIdentifier;
            points = player.points;
            stats = player.gameStatistics;
            Task = player.DailyTask != null ? new(player.DailyTask) : null;
            AvatarData = player.avatar !=null ? new(player.name ,player.avatar): null;
            if (_playerDataEmotionList == null || _playerDataEmotionList.Count == 0) playerDataEmotionList = new List<Emotion> { Emotion.Blank, Emotion.Love, Emotion.Playful, Emotion.Joy, Emotion.Sorrow, Emotion.Anger, Emotion.Blank };
            if (daysBetweenInput == null) daysBetweenInput = "0";

            if (CharacterSpecConfig.Instance.AllowTestCharacters) SetTestCharacterIds();
        }

        public void UpdateCustomCharacter(CustomCharacter character)
        {
            if (character == null) return;

            Patch();

            if (_characterList.Contains(character)) ServerManager.Instance.StartUpdatingCustomCharacterToServer(character);
            else Storefront.Get().SavePlayerData(this, null);
        }


        public void BuildCharacterLists(List<CustomCharacter> customCharacters)
        {
            List<CustomCharacter> newCustomCharacters = new();

            // Getting base characters from data store
            DataStore store = Storefront.Get();
            ReadOnlyCollection<BaseCharacter> baseCharacters = null;
            store.GetAllBaseCharacterYield(result => baseCharacters = result);

            // Checking base character is set for custom characters
            foreach (CustomCharacter character in customCharacters)
            {
                if ((!CharacterSpecConfig.Instance.AllowTestCharacters) && character.IsTestCharacter()) continue;
                if (SetBaseCharacter(baseCharacters, character)) newCustomCharacters.Add(character);
            }

            _characterList = newCustomCharacters;
            Debug.LogWarning(_characterList.Count + " : " + _characterList[0].ServerID);

            if (CharacterSpecConfig.Instance.AllowTestCharacters)
            {
                // If test character list wasn't serialized getting them from data store
                if (TestCharacterList == null || TestCharacterList.Count == 0)
                {
                    // Getting test characters from data store
                    List<CustomCharacter> testCharacters = null;
                    store.GetAllDefaultCharacterYield(characters => testCharacters = characters.Where(c => c.IsTestCharacter()).ToList());
                    if (testCharacters != null && testCharacters.Count != 0) TestCharacterList = testCharacters;
                }

                // Checking base characters are set
                List<CustomCharacter> newTestCharacters = new();

                foreach (CustomCharacter character in TestCharacterList)
                {
                    if (SetBaseCharacter(baseCharacters, character)) newTestCharacters.Add(character);
                }

                TestCharacterList = newTestCharacters;
            }

            Patch();
        }


        internal void Patch()
        {
            List<CustomCharacter> charList = _characterList;
            if (CharacterSpecConfig.Instance.AllowTestCharacters && TestCharacterList != null && TestCharacterList.Count != 0) charList = charList.Concat(TestCharacterList).ToList();
            CustomCharacters = new ReadOnlyCollection<CustomCharacter>(charList).ToList();
        }


        private bool SetBaseCharacter(ReadOnlyCollection<BaseCharacter> baseCharacters, CustomCharacter character)
        {
            if (character.CharacterBase == null)
            {
                foreach (BaseCharacter item in baseCharacters)
                {
                    if (item.Id.Equals(character.Id)) character.CharacterBase = item;
                }
            }

            return character.CharacterBase != null;
        }


        private void SetTestCharacterIds()
        {
            if (!CharacterSpecConfig.Instance.AllowTestCharacters) return;

            // Getting test characters from data store and setting to SelectedTestCharacterIds
            DataStore store = Storefront.Get();
            store.GetPlayerData(UniqueIdentifier, playerData =>
            {
                if (playerData != null && playerData.SelectedTestCharacterIds != null && playerData.SelectedTestCharacterIds.Length > 0)
                {
                    SelectedTestCharacterIds = playerData.SelectedTestCharacterIds;
                }
                else
                {
                    SelectedTestCharacterIds = new int[3] { (int)CharacterID.None, (int)CharacterID.None, (int)CharacterID.None };
                }
            });
        }


        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(SelectedCharacterId)}: {SelectedCharacterId}," +
                $"{nameof(SelectedCharacterIds)}: {string.Join(",", SelectedCharacterIds)}, { nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}
