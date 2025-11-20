using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Common;
using Prg.Scripts.Common.Extensions;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2.Internal;
using Altzone.Scripts.Voting;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.Assertions;
using Newtonsoft.Json;

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

    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerData
    {
        public string Id;
        public string ClanId;
        public int SelectedCharacterId;
        public CustomCharacterListObject[] SelectedCharacterIds = new CustomCharacterListObject[3] { new(Id: CharacterID.None), new(Id: CharacterID.None), new(Id: CharacterID.None) };
        public string Name;

        private List<CustomCharacter> _characterList;

        public int DiamondSpeed = 1000;
        public int DiamondCharacterSize = 1000;
        public int DiamondAttack = 1000;
        public int DiamondDefence = 1000;
        public int DiamondHP = 1000;
        public int Eraser = 1000;

        public int BackpackCapacity;

        public PlayerTask Task = null;
        [JsonIgnore]
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
        public string UniqueIdentifier;

        public bool HasClanId => !string.IsNullOrEmpty(ClanId);

        public List<CustomCharacter> CustomCharacters { get; private set; }

        public ReadOnlyCollection<CustomCharacter> CurrentBattleCharacters
        {
            get
            {
                List<CustomCharacter> list = new();
                for (int i = 0; i < SelectedCharacterIds.Length; i++)
                {
                    string serverId = SelectedCharacterIds[i].ServerID;
                    CharacterID lId = SelectedCharacterIds[i].CharacterID;
                    bool isTestCharacter = SelectedCharacterIds[i].IsTestCharacter;

                    if (string.IsNullOrEmpty(serverId) && !isTestCharacter)
                    {
                        list.Add(CustomCharacter.CreateEmpty());
                        continue;
                    }

                    CustomCharacter character = null;
                    if (CustomCharacters != null) character = isTestCharacter ? CustomCharacters.FirstOrDefault(x => x.Id == lId) : CustomCharacters.FirstOrDefault(x => x.ServerID == serverId);
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

        [JsonConstructor]
        private PlayerData()
        {

        }

        public PlayerData(string id, string clanId, int currentCustomCharacterId, string[]currentBattleCharacterIds, string name, int backpackCapacity, string uniqueIdentifier, List<CustomCharacter> characters)
        {
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(clanId.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(currentCustomCharacterId >= 0);
            Assert.IsTrue(name.IsSet());
            Assert.IsTrue(backpackCapacity >= 0);
            Assert.IsTrue(uniqueIdentifier.IsSet());
            Id = id;
            ClanId = clanId ?? string.Empty;
            SelectedCharacterId = currentCustomCharacterId;
            if(characters != null)BuildCharacterLists(characters);
            BuildSelectedCharacterList(currentBattleCharacterIds);
            Name = name;
            BackpackCapacity = backpackCapacity;
            UniqueIdentifier = uniqueIdentifier;
        }

        public PlayerData(ServerPlayer player, bool limited = false)
        {
            Assert.IsTrue(player._id.IsSet());
            Assert.IsTrue(player.clan_id.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(player.currentCustomCharacterId >= 0);
            Assert.IsTrue(player.name.IsSet());
            if (!limited) Assert.IsTrue(player.backpackCapacity >= 0);
            Assert.IsTrue(player.uniqueIdentifier.IsSet());
            Id = player._id;
            ClanId = player.clan_id ?? string.Empty;
            SelectedCharacterId = (int)(player.currentAvatarId == null ? 0 : player.currentAvatarId);
            string noCharacter = ((int)CharacterID.None).ToString();
            if (!limited) BuildSelectedCharacterList(player.battleCharacter_ids);
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
            Assert.IsTrue(player._id.IsSet());
            Assert.IsTrue(player.clan_id.IsNullOEmptyOrNonWhiteSpace());
            //Assert.IsTrue(player.currentCustomCharacterId >= 0);
            Assert.IsTrue(player.name.IsSet());
            Assert.IsTrue(player.backpackCapacity >= 0);
            Assert.IsTrue(player.uniqueIdentifier.IsSet());
            Id = player._id;
            ClanId = player.clan_id ?? string.Empty;
            SelectedCharacterId = (int)(player.currentAvatarId == null ? 0 : player.currentAvatarId);
            string noCharacter = ((int)CharacterID.None).ToString();
            BuildSelectedCharacterList(player.battleCharacter_ids);
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

            if (CharacterSpecConfig.Instance.AllowTestCharacters)
            {
                List<CustomCharacter> testCharacters = null;
                store.GetAllDefaultCharacterYield(characters => testCharacters = characters.Where(c => c.IsTestCharacter()).ToList());
                if (testCharacters != null && testCharacters.Count != 0) customCharacters = customCharacters.Concat(testCharacters).ToList();
            }

            // Checking base character is set for custom characters
            foreach (CustomCharacter character in customCharacters)
            {
                if ((!CharacterSpecConfig.Instance.AllowTestCharacters) && character.IsTestCharacter()) continue;
                if (SetBaseCharacter(baseCharacters, character)) newCustomCharacters.Add(character);
            }

            _characterList = newCustomCharacters;

            Patch();
        }


        internal void Patch()
        {
            List<CustomCharacter> charList = _characterList;
            //if (CharacterSpecConfig.Instance.AllowTestCharacters && TestCharacterList != null && TestCharacterList.Count != 0) charList = charList.Concat(TestCharacterList).ToList();
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

        private void BuildSelectedCharacterList(string[] server_ids)
        {
            for(int i=0; i < 3; i++)
            {
                string serverid = null;
                if(server_ids.Length > i) serverid = server_ids[i];

                if (_characterList == null) _characterList = new();
                CustomCharacter character = _characterList.FirstOrDefault(c => c.ServerID == serverid);
                if (i < SelectedCharacterIds.Length)
                {
                    if (SelectedCharacterIds[i] != null) SelectedCharacterIds[i].SetData(serverid, character == null ? CharacterID.None :character.Id);
                    else SelectedCharacterIds[i] = new(serverid, character == null ? CharacterID.None : character.Id);
                }
                else SelectedCharacterIds.Append(new(serverid, character == null ? CharacterID.None : character.Id));
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ClanId)}: {ClanId}, {nameof(SelectedCharacterId)}: {SelectedCharacterId}," +
                $"{nameof(SelectedCharacterIds)}: {string.Join<CustomCharacterListObject>(",", SelectedCharacterIds)}, { nameof(Name)}: {Name}, {nameof(BackpackCapacity)}: {BackpackCapacity}, {nameof(UniqueIdentifier)}: {UniqueIdentifier}";
        }
    }
}
