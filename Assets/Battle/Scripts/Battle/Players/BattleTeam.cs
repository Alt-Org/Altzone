using System.Collections.Generic;

namespace Battle.Scripts.Battle.Players
{
    interface IReadOnlyBattleTeam
    {
        public BattleTeamNumber TeamNumber { get; }
        public IReadOnlyList<IReadOnlyBattlePlayer> Players { get; }
        public IReadOnlyBattleTeam OtherTeam { get; }
}

    internal class BattleTeam : IReadOnlyBattleTeam
    {
        public BattleTeamNumber TeamNumber => _teamNumber;
        public List<BattlePlayer> Players => _players;
        IReadOnlyList<IReadOnlyBattlePlayer> IReadOnlyBattleTeam.Players => _players;
        public IReadOnlyBattleTeam OtherTeam => _otherTeam;

        public BattleTeam(BattleTeamNumber teamNumber)
        {
            _teamNumber = teamNumber;
            _players = new();
        }

        public void SetOtherTeam(BattleTeam otherTeam)
        {
            _otherTeam = otherTeam;
        }

        private readonly BattleTeamNumber _teamNumber;
        private readonly List<BattlePlayer> _players;
        private BattleTeam _otherTeam;
    }
}
