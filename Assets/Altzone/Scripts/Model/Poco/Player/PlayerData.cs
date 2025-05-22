using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
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
        [ForeignKey(nameof(CustomCharacter)), Optional] public string[] SelectedCharacterIds = new string[3];
        [Unique] public string Name;

        private List<CustomCharacter> _characterList;

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
                foreach (var id in SelectedCharacterIds)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    CustomCharacter character = CustomCharacters.FirstOrDefault(x => x.ServerID == id);
                    if(character == null) continue;
                    list.Add(character);
                }
                while(list.Count < 3)
                {
                    list.Add(CustomCharacter.CreateEmpty());
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

        public PlayerData(string id, string clanId, int currentCustomCharacterId, string[]currentBattleCharacterIds, string name, int backpackCapacity, string uniqueIdentifier)
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
            if(!limited)SelectedCharacterIds = (player?.battleCharacter_ids == null || player.battleCharacter_ids.Length < 3) ? new string[3] {"0","0","0"} : player.battleCharacter_ids;
            Name = player.name;
            if (!limited) BackpackCapacity = player.backpackCapacity;
            UniqueIdentifier = player.uniqueIdentifier;
            points = player.points;
            stats = player.gameStatistics;
            Task = player.DailyTask != null ? new(player.DailyTask) : null;
            AvatarData = player.avatar != null ? new(player.name, player.avatar) : null;
            if (!limited) Task = player.DailyTask != null ? new(player.DailyTask): null;
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
            SelectedCharacterIds = player?.battleCharacter_ids == null || player.battleCharacter_ids.Length < 3 ? new string[3] { "0", "0", "0" } : player.battleCharacter_ids;
            Name = player.name;
            BackpackCapacity = player.backpackCapacity;
            UniqueIdentifier = player.uniqueIdentifier;
            points = player.points;
            stats = player.gameStatistics;
            Task = player.DailyTask != null ? new(player.DailyTask) : null;
            AvatarData = player.avatar !=null ? new(player.name ,player.avatar): null;
            if (_playerDataEmotionList == null || _playerDataEmotionList.Count == 0) playerDataEmotionList = new List<Emotion> { Emotion.Blank, Emotion.Love, Emotion.Playful, Emotion.Joy, Emotion.Sorrow, Emotion.Anger, Emotion.Blank };
            if (daysBetweenInput == null) daysBetweenInput = "0";
        }

        public void UpdateCustomCharacter(CustomCharacter character)
        {
            if (character == null) return;

            bool characterInCharacterList = false;
            int i = 0;
            foreach (CustomCharacter item in _characterList)
            {
                if (item.Id != character.Id)
                {
                    i++;
                    continue;
                }

                _characterList[i] = character;
                characterInCharacterList = true;
                break;
            }
            Patch();
            if (characterInCharacterList) ServerManager.Instance.StartUpdatingCustomCharacterToServer(character);
        }


        public void BuildCharacterLists(List<CustomCharacter> customCharacters)
        {
            _characterList = customCharacters;
            Debug.LogWarning(_characterList.Count + " : " + _characterList[0].ServerID);
            Patch();
        }


        internal void Patch()
        {
            CustomCharacters = new ReadOnlyCollection<CustomCharacter>(_characterList).ToList();
        }


        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(SelectedCharacterId)}: {SelectedCharacterId}," +
                $"{nameof(SelectedCharacterIds)}: {string.Join(",", SelectedCharacterIds)}, { nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}
