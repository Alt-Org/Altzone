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

        public bool BounceOnBallShieldCollision => true;

        public void OnBallShieldCollision()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));

            //Increase vacuum strength
            _teammateVacuumStrength += _teammateVacuumStrengthIncrement;

            //Increase vacuum duration
            _teammateVacuumDuration += _teammateVacuumDurationIncrement;

            //Timer on
            _teammateVacuumState = true;

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "teammateVacuumStrength is " + _teammateVacuumStrength, _syncedFixedUpdateClock.UpdateCount));
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "TeammateVacuum on", _syncedFixedUpdateClock.UpdateCount));
        }

        public void OnBallShieldBounce()
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(_syncedFixedUpdateClock.UpdateCount+5, -10, () =>
            {
                if (_shieldTurn == 0)
                {
                    _shieldTurn++;
                    ShieldFlipper();
                    Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "_shieldTurn set to 1", _syncedFixedUpdateClock.UpdateCount));
                }
                else
                {
                    _shieldTurn = 0;
                    ShieldFlipper();
                    Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "_shieldTurn set to 0", _syncedFixedUpdateClock.UpdateCount));
                }
            });
        }

        private IDriver _driver;

        private int _shieldTurn = 0;

        private Transform _teammateTransform;

        private bool _teammateVacuumState = false;
        private int _teammateVacuumTimer;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS CONFLUENT] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time


        private void Start()
        {
            _teammateVacuumStrength -= _teammateVacuumStrengthIncrement;
            _teammateVacuumDuration -= _teammateVacuumDurationIncrement;

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);

            // debug
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PlayerActor playerActor = transform.parent.GetComponentInParent<PlayerActor>();

            int teamnumber = PhotonBattle.NoTeamValue;

            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.PlayerActor == playerActor)
                {
                    teamnumber = driver.TeamNumber;
                    _driver = driver;
                }
            }

            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.TeamNumber == teamnumber && driver.PlayerActor != playerActor)
                {
                    _teammateTransform = driver.ActorShieldTransform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_teammateVacuumState == true && _teammateVacuumDuration > _teammateVacuumTimer)
            {
                _teammateVacuumTimer++;
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "TeammateVacuumTimer is " + _teammateVacuumTimer, _syncedFixedUpdateClock.UpdateCount));

                //_driver.ActorShieldTransform on oma sijainti
                //_teammateTransform on kaverin sijainti

                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Own position is " + _driver.ActorShieldTransform.position, _syncedFixedUpdateClock.UpdateCount));
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Teammate position is " + _teammateTransform.position, _syncedFixedUpdateClock.UpdateCount));

                //Laske tiimikaveri suhteessa positioon eli vektori C eli B-A
                //Normalisoi C vektori
                //Kerro C vektori (voimalla / sekunnilla jaettuna frameihin) eli saat siirtymän
                Vector3 newPosition = _driver.ActorShieldTransform.position + (_teammateTransform.position - _driver.ActorShieldTransform.position).normalized * (_teammateVacuumStrength/(float)SyncedFixedUpdateClock.UPDATES_PER_SECOND);

                //Siirrä itseä lähemmäs tiimikaveria timerin ajan strengthin perusteella
                _driver.ActorCharacterTransform.position = newPosition;
                _driver.ActorShieldTransform.position = newPosition;
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
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is set to _shieldGumball", _syncedFixedUpdateClock.UpdateCount));
            }
            else
            {
                _shieldManager.SetShield(_shieldPopped);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is set to _shieldPopped", _syncedFixedUpdateClock.UpdateCount));
            }
        }
    }
}
