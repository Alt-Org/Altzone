using UnityEngine;
using Prg.Scripts.Common.PubSub;

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
        }

        public bool BounceOnBallShieldCollision => true;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS TRICKSTER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        public void OnBallShieldCollision()
        {
            _battleDebugLogger.LogInfo("OnBallShieldCollision called");

            //Increase vacuum strength
            _teammateVacuumStrength += _teammateVacuumStrengthIncrement;

            //Increase vacuum duration
            _teammateVacuumDuration += _teammateVacuumDurationIncrement;

            //Timer on
            _teammateVacuumState = true;

            _battleDebugLogger.LogInfo("teammateVacuumStrength is ");
            _battleDebugLogger.LogInfo("TeammateVacuum on");
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

        private BattleDebugLogger _battleDebugLogger;

        private int _shieldTurn = 0;

        private Transform _actorShieldTransform;
        private Transform _actorCharacterTransform;
        private Transform _teammateShieldTransform;

        private bool _teammateVacuumState = false;
        private int _teammateVacuumTimer;

        private void Start()
        {
            _teammateVacuumStrength -= _teammateVacuumStrengthIncrement;
            _teammateVacuumDuration -= _teammateVacuumDurationIncrement;

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PlayerActor playerActor = _battlePlayer.PlayerActor;
            int teamnumber = _battlePlayer.PlayerPosition;

            _actorShieldTransform = _battlePlayer.PlayerShieldManager.transform;
            _actorCharacterTransform = _battlePlayer.PlayerCharacter.transform;
            _teammateShieldTransform = _battlePlayer.Teammate.PlayerShieldManager.transform;
        }

        private void FixedUpdate()
        {
            if (_teammateVacuumState == true && _teammateVacuumDuration > _teammateVacuumTimer)
            {
                _teammateVacuumTimer++;
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "TeammateVacuumTimer is {0}", _teammateVacuumTimer, _syncedFixedUpdateClock.UpdateCount));
                //{n} kertoo muuttujan indeksin

                //_actorShieldTransform on oma sijainti
                //_teammateShieldTransform on kaverin sijainti

                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Own position is " + _actorShieldTransform.position));
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Teammate position is " + _teammateShieldTransform.position));

                //Laske tiimikaveri suhteessa positioon eli vektori C eli B-A
                //Normalisoi C vektori
                //Kerro C vektori (voimalla / sekunnilla jaettuna frameihin) eli saat siirtymän
                Vector3 newPosition = _actorShieldTransform.position + (_teammateShieldTransform.position - _actorShieldTransform.position).normalized * (_teammateVacuumStrength / (float)SyncedFixedUpdateClock.UpdatesPerSecond);

                //Siirrä itseä lähemmäs tiimikaveria timerin ajan strengthin perusteella
                _actorCharacterTransform.position = newPosition;
                _actorShieldTransform.position = newPosition;
            }
            else {
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
