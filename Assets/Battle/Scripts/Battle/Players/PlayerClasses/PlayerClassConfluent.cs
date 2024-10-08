using UnityEngine;
using Prg.Scripts.Common.PubSub;
using System.Runtime.CompilerServices;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassConfluent : MonoBehaviour, IPlayerClass
    {
        // Serialized Fields
        [SerializeField] private GameObject _shieldGumball;
        [SerializeField] private GameObject _shieldPopped;
        [SerializeField] private ShieldManager _shieldManager;

        [Header("Teammate Vacuum Strength")]
        [SerializeField] private float _teammateVacuumStrength;
        [SerializeField] private float _teammateVacuumStrengthIncrement;

        [Header("Teammate Vacuum Timer")]
        [SerializeField] private int _teammateVacuumDuration;
        [SerializeField] private int _teammateVacuumDurationIncrement;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
            _battleDebugLogger = new BattleDebugLogger(this);
            _teammateVacuumStrength -= _teammateVacuumStrengthIncrement;
            _teammateVacuumDuration -= _teammateVacuumDurationIncrement;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);

            PlayerActor playerActor = _battlePlayer.PlayerActor;

            _actorShieldTransform = _battlePlayer.PlayerShieldManager.transform;
            _actorCharacterTransform = _battlePlayer.PlayerCharacter.transform;

            //Important check
            _hasTeammate = _battlePlayer.Teammate != null;

            if (_hasTeammate)
            {
                _teammateShieldTransform = _battlePlayer.Teammate.PlayerShieldManager.transform;
            }
        }

        public bool BounceOnBallShieldCollision => true;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS TRICKSTER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        public void OnBallShieldCollision()
        {
            _battleDebugLogger.LogInfo("OnBallShieldCollision called");

            if (_hasTeammate)
            {
                //Increase vacuum strength
                _teammateVacuumStrength += _teammateVacuumStrengthIncrement;

                //Increase vacuum duration
                _teammateVacuumDuration += _teammateVacuumDurationIncrement;

                //Timer on
                _teammateVacuumState = true;

                _battleDebugLogger.LogInfo("teammateVacuumStrength is " + _teammateVacuumStrength);
                _battleDebugLogger.LogInfo("TeammateVacuum on");
            }
        }

        public void OnBallShieldBounce()
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(_syncedFixedUpdateClock.UpdateCount+5, -10, () =>
            {
                if (_shieldTurn == 0)
                {
                    _shieldTurn++;
                    ShieldFlipper();
                    _battleDebugLogger.LogInfo("_shieldTurn set to 1");
                }
                else
                {
                    _shieldTurn = 0;
                    ShieldFlipper();
                    _battleDebugLogger.LogInfo("_shieldTurn set to 0");
                }
            });
        }

        private IReadOnlyBattlePlayer _battlePlayer;

        private IPlayerDriver _driver;

        private bool _hasTeammate;

        private int _shieldTurn = 0;

        private Transform _actorShieldTransform;
        private Transform _actorCharacterTransform;
        private Transform _teammateShieldTransform;

        private bool _teammateVacuumState = false;
        private int _teammateVacuumTimer;

        private BattleDebugLogger _battleDebugLogger;

        private void Start()
        {}

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
        }

        private void FixedUpdate()
        {
            if (!_hasTeammate) return;

            if (_teammateVacuumState == true && _teammateVacuumDuration > _teammateVacuumTimer)
            {
                _teammateVacuumTimer++;
                _battleDebugLogger.LogInfo("TeammateVacuumTimer is " + _teammateVacuumTimer);
                //{n} marks the index

                //_actorShieldTransform tells own position
                //_teammateShieldTransform tells teammate position

                _battleDebugLogger.LogInfo("Own position is " + _actorShieldTransform.position);
/*                _battleDebugLogger.LogInfo("Teammate position is " + _teammateShieldTransform.position);*/

                //Calculate teammate position relative to own position C eli B-A
                //Normalize C vector
                //Multiply C vector (power / seconds divided to frames) yields distance moved
                Vector3 newPosition = _actorShieldTransform.position + (_teammateShieldTransform.position - _actorShieldTransform.position).normalized * (_teammateVacuumStrength / (float)SyncedFixedUpdateClock.UpdatesPerSecond);

                //Move self closer to teammate based on timers time and movement strength
                //_actorCharacterTransform.position = newPosition;
                _actorShieldTransform.position = newPosition;
            }
            else
            {
                _teammateVacuumState = false;
                _teammateVacuumTimer = 0;
            }
        }

        private void ShieldFlipper() {

            if (_shieldTurn == 0)
            {
                _shieldManager.SetShield(_shieldGumball);
                _battleDebugLogger.LogInfo("Shield is set to _shieldGumball");
            }
            else
            {
                _shieldManager.SetShield(_shieldPopped);
                _battleDebugLogger.LogInfo("Shield is set to _shieldPopped");
            }
        }
    }
}
