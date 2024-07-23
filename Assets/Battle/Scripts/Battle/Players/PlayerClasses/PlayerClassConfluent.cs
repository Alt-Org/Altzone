using System;
using UnityEngine;
using Prg.Scripts.Common.PubSub;



namespace Battle.Scripts.Battle.Players
{
    internal class PlayerClassConfluent : MonoBehaviour, IPlayerClass
    {

        // Variables

        [SerializeField] private GameObject _shieldGumball;
        [SerializeField] private GameObject _shieldPopped;
        [SerializeField] private ShieldManager _shieldManager;
        [SerializeField] private int _teammateVacuumDuration;
        [SerializeField] private int _teammateVacuumStrength;
        [SerializeField] private int _teammateVacuumStrengthIncrement;




        private int _shieldTurn = 0;


        public bool BounceOnBallShieldCollision => true;


        public void OnBallShieldCollision()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));


            // Laita timer päälle
            // Laita voimaa kovemmalle
            _teammateVacuumTimer = _teammateVacuumDuration;
            
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





        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS CONFLUENT] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        private Transform _teammateTransform;

        private int _teammateVacuumTimer;

        
        
        


        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);

        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PlayerActor playerActor = transform.parent.GetComponentInParent<PlayerActor>();

            int teamnumber = PhotonBattle.NoTeamValue;

            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.PlayerActor == playerActor) teamnumber = driver.TeamNumber;
            }



            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.TeamNumber == teamnumber && driver.PlayerActor != playerActor);
            }



        }



        private void FixedUpdate()
        {


            //Siirrä tiimikaveria lähemmäs timerin ajan strengthin perusteella


            //transform on oma sijainti
            

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
