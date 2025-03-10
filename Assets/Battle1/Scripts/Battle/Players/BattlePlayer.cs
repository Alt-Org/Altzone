using Altzone.Scripts.Model.Poco.Game;
using Battle1.Scripts.Battle.Players.PlayerClasses;

namespace Battle1.Scripts.Battle.Players
{
    interface IReadOnlyBattlePlayer
    {
        // Info
        public int PlayerPosition { get; }
        public BattleCharacter BattleCharacter { get; }
        public bool IsBot { get; }

        // Team
        public IReadOnlyBattleTeam BattleTeam { get; }
        public IReadOnlyBattlePlayer Teammate {  get; }

        public  IPlayerDriver PlayerDriver { get; }

        // PlayerActor Parts
        public PlayerActor PlayerActor { get; }
        public IPlayerClass PlayerClass { get; }
        public ShieldManager PlayerShieldManager { get; }
        public PlayerCharacter PlayerCharacter { get; }
        public PlayerSoul PlayerSoul { get; }
    }

    internal class BattlePlayer : IReadOnlyBattlePlayer
    {
        // Info
        public int PlayerPosition => _playerPosition;
        public BattleCharacter BattleCharacter => _battleCharacter;
        public bool IsBot => _isBot;

        // Team
        public IReadOnlyBattleTeam BattleTeam => _battleTeam;
        public IReadOnlyBattlePlayer Teammate => _teammate;

        public IPlayerDriver PlayerDriver => _playerDriver;

        // PlayerActor Parts
        public PlayerActor PlayerActor => _playerActor;
        public IPlayerClass PlayerClass => _playerClass;
        public ShieldManager PlayerShieldManager => _playerShieldManager;
        public PlayerCharacter PlayerCharacter => _playerCharacter;
        public PlayerSoul PlayerSoul => _playerSoul;

        public BattlePlayer(int playerPosition, BattleCharacter battleCharacter, bool isBot, IPlayerDriver playerDriver)
        {
            _playerPosition = playerPosition;
            _battleCharacter = battleCharacter;
            _isBot = isBot;

            _playerDriver = playerDriver;
        }

        public void SetBattleTeam(BattleTeam battleTeam)
        {
            _battleTeam = battleTeam;
        }

        public void SetTeammate(BattlePlayer teammate)
        {
            _teammate = teammate;
        }

        public void SetPlayerActorParts(PlayerActor playerActor, IPlayerClass playerClass, ShieldManager playerShieldManager, PlayerCharacter playerCharacter, PlayerSoul playerSoul)
        {
            _playerActor = playerActor;
            _playerClass = playerClass;
            _playerShieldManager = playerShieldManager;
            _playerCharacter = playerCharacter;
            _playerSoul = playerSoul;
        }

        private readonly int _playerPosition;
        private readonly BattleCharacter _battleCharacter;
        private readonly bool _isBot;

        private IReadOnlyBattleTeam _battleTeam;
        private IReadOnlyBattlePlayer _teammate;

        private readonly IPlayerDriver _playerDriver;

        private PlayerActor _playerActor;
        private IPlayerClass _playerClass;
        private ShieldManager _playerShieldManager;
        private PlayerCharacter _playerCharacter;
        private PlayerSoul _playerSoul;
    }
}
