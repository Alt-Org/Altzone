using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Player;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        [PrimaryKey] public string _id;
        private string _name;
        [ForeignKey(nameof(PlayerData)), Mandatory] public string PlayerDataId;
        [ForeignKey(nameof(RaidRoom)), Optional] public string RaidRoomId;
        public ClanMemberRole Role;
        private ServerPlayer _player;

        private int _leaderBoardWins = 0;
        private int _leaderBoardCoins = 0;

        public string Id { get => _id; }
        public string Name { get => _name; }
        public int LeaderBoardWins { get => _leaderBoardWins;}
        public int LeaderBoardCoins { get => _leaderBoardCoins;}

        public ClanMember(ServerPlayer player)
        {
            _id = player._id;
            _name = player.name;
            _player = player;
        }

        public PlayerData GetPlayerData()
        {
            return new(_player, true);
        }

        public void Update(int wins, int coins)
        {
            _leaderBoardWins = wins;
            _leaderBoardCoins = coins;
        }
        public void AddWin()
        {
            _leaderBoardWins++;
        }

        public void AddCoins(int amount)
        {
            _leaderBoardCoins = +amount;
        }
    }
}
