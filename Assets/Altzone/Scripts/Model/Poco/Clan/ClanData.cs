using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Prg.Scripts.Common.Extensions;
using Altzone.Scripts.Store;
using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.Assertions;
using Newtonsoft.Json;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanData
    {
        public string Id;
        public string Name;
        public string Tag;
        public string Phrase;
        private int _gameCoins;
        public int Points;
        public bool IsOpen;

        public List<ClanValues> Values = new();
        public List<HeartPieceData> ClanHeartPieces = new();
        public List<ClanRoles> ClanRoles = new ();

        [JsonIgnore]
        public ClanInventory Inventory = new();
        [JsonIgnore]
        public List<PollData> Polls = new();

        public AdStoreObject _adData;
        public AdStoreObject AdData {
            get { return _adData != null ? _adData : _adData = new(null, null); }
            set { _adData = value; CallAdDataUpdate(); } }
        [JsonIgnore]
        public List<ClanMember> Members = new();
        public List<RaidRoom> Rooms = new();

        public ClanAge ClanAge;
        public Language Language;
        public Goals Goals;

        public int GameCoins { get => _gameCoins; set { _gameCoins = value; CallDataUpdate(); } }

        public delegate void ClanInfoUpdated();
        public static event ClanInfoUpdated OnClanInfoUpdated;

        public delegate void AdDataUpdated();
        public static event AdDataUpdated OnAdDataUpdated;

        [JsonConstructor]
        private ClanData()
        {

        }
        
        public ClanData(string id, string name, string tag, int gameCoins)
        {
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(name.IsSet());
            Assert.IsTrue(tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(gameCoins >= 0);
            Id = id;
            Name = name;
            Tag = tag ?? string.Empty;
            _gameCoins = gameCoins;
        }

        public ClanData(ServerClan clan)
        {
            Assert.IsTrue(clan._id.IsSet());
            Assert.IsTrue(clan.name.IsSet());
            Assert.IsTrue(clan.tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(clan.gameCoins >= 0);
            Id = clan._id;
            Name = clan.name;
            Tag = clan.tag ?? string.Empty;
            Phrase = clan.phrase ?? string.Empty;
            _gameCoins = clan.gameCoins;
            Points = clan.points;
            foreach (string point in clan.labels)
            {
                Values.Add((ClanValues)Enum.Parse(typeof(ClanValues), string.Concat(point[0].ToString().ToUpper(), point.AsSpan(1).ToString()).Replace("ä", "a").Replace("ö","o").Replace("+", "").Replace(" ", "")));
            }
            ClanAge = clan.ageRange;
            Language = clan.language;
            Goals = clan.goal;
            ClanHeartPieces = new();
            int i=0;
            if(clan.clanLogo != null)
            foreach(var piece in clan.clanLogo.pieceColors)
            {
                if (!ColorUtility.TryParseHtmlString("#" + piece, out Color colour)) colour = Color.white;
                    ClanHeartPieces.Add(new(i, colour));

                i++;
            }
            IsOpen = clan.isOpen;
            if (clan.polls != null) Polls = clan.polls;
            ClanRoles = clan.roles;
        }

        public void UpdateClanData(ServerClan clan)
        {
            Assert.IsTrue(clan._id.IsSet());
            Assert.IsTrue(clan.name.IsSet());
            Assert.IsTrue(clan.tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(clan.gameCoins >= 0);
            Id = clan._id;
            Name = clan.name;
            Tag = clan.tag ?? string.Empty;
            Phrase = clan.phrase ?? string.Empty;
            _gameCoins = clan.gameCoins;
            Points = clan.points;
            Values = new();
            foreach (string point in clan.labels)
            {
                Values.Add((ClanValues)Enum.Parse(typeof(ClanValues), string.Concat(point[0].ToString().ToUpper(), point.AsSpan(1).ToString()).Replace("ä", "a").Replace("ö", "o").Replace("+", "").Replace(" ", "")));
            }
            ClanAge = clan.ageRange;
            Language = clan.language;
            Goals = clan.goal;
            ClanHeartPieces = new();
            int i = 0;
            if (clan.clanLogo != null)
                foreach (var piece in clan.clanLogo.pieceColors)
                {
                    if (!ColorUtility.TryParseHtmlString("#" + piece, out Color colour)) colour = Color.white;
                    ClanHeartPieces.Add(new(i, colour));

                    i++;
                }
            IsOpen = clan.isOpen;
            if (clan.polls != null) Polls = clan.polls;
            else if (Polls == null) Polls = new();
            Rooms = new();
        }

        public void CallDataUpdate()
        {
            OnClanInfoUpdated?.Invoke();
        }

        public void CallAdDataUpdate()
        {
            OnAdDataUpdated?.Invoke();
        }

        public override string ToString()
        {
            return $"  {nameof(Id)}: {Id}, " +
                   $"  {nameof(Name)}: {Name}, " +
                   $"  {nameof(Tag)}: {Tag}, " +
                   $"  {nameof(GameCoins)}: {GameCoins}" +
                   $", {nameof(Inventory)}: {Inventory}" +
                   $", {nameof(Polls)}: {Polls.Count}" +
                   $", {nameof(Members)}: {Members.Count}, " +
                   $"  {nameof(Rooms)}: {Rooms.Count}";
        }
    }
}
