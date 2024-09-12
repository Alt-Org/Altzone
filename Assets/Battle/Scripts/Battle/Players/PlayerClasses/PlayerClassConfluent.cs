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

        public bool BounceOnBallShieldCollision => true;

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

        public void OnBallShieldCollision()
        {
            _battleDebugLogger.LogInfo("OnBallShieldCollision called");

            //Increase vacuum strength
            _teammateVacuumStrength += _teammateVacuumStrengthIncrement;

            //Increase vacuum duration
            _teammateVacuumDuration += _teammateVacuumDurationIncrement;

            //Timer on
            _teammateVacuumState = true;

            _battleDebugLogger.LogInfo("teammateVacuumStrength is " + _teammateVacuumStrength);
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

        private int _shieldTurn = 0;

        private Transform _teammateTransform;

        private bool _teammateVacuumState = false;
        private int _teammateVacuumTimer;

        //Tämä tarvitaan ExecuteOnUpdateen
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        private BattleDebugLogger _battleDebugLogger;

        private void Start()
        {
            _teammateVacuumStrength -= _teammateVacuumStrengthIncrement;
            _teammateVacuumDuration -= _teammateVacuumDurationIncrement;

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);

            // debug

            _battleDebugLogger = new BattleDebugLogger(this);

            // debug test
            _battleDebugLogger.LogInfo("Test");

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            /* broken code pls fix
            PlayerActor playerActor = transform.parent.GetComponentInParent<PlayerActor>();

            int teamnumber = PhotonBattle.NoTeamValue;

            foreach (IPlayerDriver driver in data.AllDrivers)
            {
                if (driver.PlayerActor == playerActor)
                {
                    teamnumber = driver.TeamNumber;
                    _driver = driver;
                }
            }

            foreach (IPlayerDriver driver in data.AllDrivers)
            {
                if (driver.TeamNumber == teamnumber && driver.PlayerActor != playerActor)
                {
                    _teammateTransform = driver.ActorShieldTransform;
                }
            }
            */
        }

        private void FixedUpdate()
        {
            if (_teammateVacuumState == true && _teammateVacuumDuration > _teammateVacuumTimer)
            {
                _teammateVacuumTimer++;
                _battleDebugLogger.LogInfo("TeammateVacuumTimer is {0}", _teammateVacuumTimer);

                //{n} kertoo muuttujan indeksin

                //_driver.ActorShieldTransform on oma sijainti
                //_teammateTransform on kaverin sijainti

                /* broken code pls fix
                _battleDebugLogger.LogInfo("Own position is " + _driver.ActorShieldTransform.position);
                _battleDebugLogger.LogInfo("Teammate position is " + _teammateTransform.position);

                //Laske tiimikaveri suhteessa positioon eli vektori C eli B-A
                //Normalisoi C vektori
                //Kerro C vektori (voimalla / sekunnilla jaettuna frameihin) eli saat siirtymän
                Vector3 newPosition = _driver.ActorShieldTransform.position + (_teammateTransform.position - _driver.ActorShieldTransform.position).normalized * (_teammateVacuumStrength/(float)SyncedFixedUpdateClock.UPDATES_PER_SECOND);

                //Siirrä itseä lähemmäs tiimikaveria timerin ajan strengthin perusteella
                _driver.ActorCharacterTransform.position = newPosition;
                _driver.ActorShieldTransform.position = newPosition;
                */
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
