using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Voting;
using UnityEngine;
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
        private int _gameCoins;
        public int Points;
        public bool IsOpen;

        public List<ClanValues> Values = new();
        public List<HeartPieceData> ClanHeartPieces = new();
        public ClanRoleRights[] ClanRights = new ClanRoleRights[3];

        public ClanInventory Inventory = new();

        public List<PollData> Polls = new();

        public List<ClanMember> Members = new();
        public List<RaidRoom> Rooms = new();

        public ClanAge ClanAge;
        public Language Language;
        public Goals Goals;

        public int GameCoins { get => _gameCoins; set { _gameCoins = value; CallDataUpdate(); } }

        public delegate void ClanInfoUpdated();
        public static event ClanInfoUpdated OnClanInfoUpdated;

        public ClanData(string id, string name, string tag, int gameCoins)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(name.IsMandatory());
            Assert.IsTrue(tag.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(gameCoins >= 0);
            Id = id;
            Name = name;
            Tag = tag ?? string.Empty;
            _gameCoins = gameCoins;
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
        }

        public void CallDataUpdate()
        {
            OnClanInfoUpdated?.Invoke();
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Tag)}: {Tag}, {nameof(GameCoins)}: {GameCoins}" +
                   $", {nameof(Inventory)}: {Inventory}" +
                   $", {nameof(Polls)}: {Polls.Count}" +
                   $", {nameof(Members)}: {Members.Count}, {nameof(Rooms)}: {Rooms.Count}";
        }
    }
}
