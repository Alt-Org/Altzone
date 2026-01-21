using System;
using System.Diagnostics.CodeAnalysis;
using Assets.Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Player;
using Newtonsoft.Json;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        public string _id;
        private string _name;
        public string PlayerDataId;
        public string RaidRoomId;
        public ClanRoles Role;
        private ServerPlayer _player;

        public string ClanRoleId;

        private int _leaderBoardWins = 0;
        private int _leaderBoardCoins = 0;

        public string Id { get => _id; }
        public string Name { get => _name; }
        public int LeaderBoardWins { get => _leaderBoardWins;}
        public int LeaderBoardCoins { get => _leaderBoardCoins;}

        [JsonConstructor]
        private ClanMember() { }

        public ClanMember(ServerPlayer player)
        {
            _id = player._id;
            _name = player.name;
            _player = player;

            ClanRoleId = player.clanRole_id;
        }

        public AvatarData AvatarData =>
            _player.avatar != null ? new AvatarData(Name, _player.avatar) : null;

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
