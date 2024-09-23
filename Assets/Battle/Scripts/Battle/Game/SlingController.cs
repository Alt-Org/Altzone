using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Prg.Scripts.Common.MathPlus;
using Prg.Scripts.Common.PubSub;

using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Battle.Game
{
    #region Message Classes

    internal class SlingControllerReady
    { }

    internal class BallSlinged
    {
        public BattleTeamNumber SlingingTeamNumber { get; }

        public BallSlinged(BattleTeamNumber slingingTeamNumber)
        {
            SlingingTeamNumber = slingingTeamNumber;
        }
    }

    #endregion Message Classes

    [RequireComponent(typeof(AudioSource))]
    internal class SlingController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Sling")]
        [SerializeField] private float _minDistance;
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _slingMinSpeed;
        [SerializeField] private float _slingMaxSpeed;
        [SerializeField] private int _slingDefaultSpeed;
        [SerializeField] private float _ballStartingDistance;
        [Header("Indicator")]
        [SerializeField] private float _wingMinAngleDegrees;
        [SerializeField] private float _wingMaxAngleDegrees;
        [Header("Game Objects")]
        [SerializeField] private BallHandler _ball;
        [SerializeField] private SlingIndicator _slingIndicator;
        #endregion Serialized Fields

        #region Public

        #region Public - Properties
        public bool SlingActive => _slingActive;
        #endregion Public - Properties

        #region Public - Methods

        /// <summary>
        /// <para>Activates a teams sling.</para>
        /// <para>
        /// Slings can not be activated when a sling is already active.
        /// The sling stays active until it is launched after which it is deactivated.
        /// Deactivating the sling without launching is not implemented.
        /// </para>
        /// <para>
        /// This method only effects sling on this client.
        /// </para>
        /// </summary>
        /// <param name="teamNumber">The PhotonBattle team number of the team which sling is going to be activated</param>
        public void SlingActivate(BattleTeamNumber teamNumber)
        {
            if (_slingActive)
            {
                _battleDebugLogger.LogWarning("Sling already active");
                return;
            }

            switch (teamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    _currentTeam = _teams[TeamAlpha];
                    _battleDebugLogger.LogInfo("Team alpha sling activated");
                    break;

                case BattleTeamNumber.TeamBeta:
                    _currentTeam = _teams[TeamBeta];
                    _battleDebugLogger.LogInfo("Team beta sling activated");
                    break;
            }
            _slingActive = true;
            _slingIndicator.SetPusherPosition(0);
        }

        /// <summary>
        /// <para>
        /// Launches the sling that is currently active.
        /// </para>
        /// A sling needs to be already be active.
        /// The sling is deactivated after launching.
        /// Deactivating the sling without launching is not implemented.
        /// <para>
        /// </para>
        /// <para>
        /// This method only effects sling on this client.
        /// </para>
        /// </summary>
        public void SlingLaunch()
        {
            if (!_slingActive)
            {
                _battleDebugLogger.LogWarning("Sling not active");
                return;
            }

            BattleTeamNumber teamNumber = _currentTeam.TeamNumber;
            Vector3 launchPosition;
            Vector3 launchDirection;
            float launchSpeed;

            if (_currentTeam.Distance >= 0)
            {
                _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "info (front player: (position: {0}, grid position: ({1})), back player: (position: {2}, grid position: {3}))",
                    _currentTeam.FrontPlayer.position,
                    _gridManager.WorldPointToGridPosition(_currentTeam.FrontPlayer.position),
                    _currentTeam.BackPlayer.position,
                    _gridManager.WorldPointToGridPosition(_currentTeam.BackPlayer.position)
                );

                launchSpeed = MathPlus.RemapAndClamp(_currentTeam.Distance, _minDistance, _maxDistance, _slingMinSpeed, _slingMaxSpeed);
                launchDirection = _currentTeam.LaunchDirection;
                launchPosition = _currentTeam.FrontPlayer.position + launchDirection * _ballStartingDistance;

                float launchDuration = _currentTeam.Distance / launchSpeed;

                _syncedFixedUpdateClock.ExecuteOnUpdate(_syncedFixedUpdateClock.UpdateCount + Mathf.Max(_syncedFixedUpdateClock.ToUpdates(launchDuration), 1), -1, () =>
                {
                    SlingLaunchPrivate(teamNumber, launchPosition, launchDirection, launchSpeed);
                });

                StartCoroutine(MovePusherCoroutine(launchDuration));
            }
            else
            {
                _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "players not in valid sling formation");

                teamNumber = BattleTeamNumber.NoTeam;
                launchDirection = new Vector3(0.5f, 0.5f);
                launchSpeed = _slingDefaultSpeed;
                launchPosition = Vector3.zero;

                _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "animation skipped");

                SlingLaunchPrivate(teamNumber, launchPosition, launchDirection, launchSpeed);
            }
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields

        // State
        private bool _slingActive = false;

        // Teams
        private const int TeamAlpha = 0;
        private const int TeamBeta  = 1;
        private class Team
        {
            public BattleTeamNumber TeamNumber;
            public List<Transform> List = new();
            public Transform FrontPlayer;
            public Transform BackPlayer;
            public float Distance;
            public Vector3 LaunchDirection;
        };
        private Team[] _teams;
        private Team _currentTeam;

        // Components
        private AudioSource _audioSource;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        #endregion Private - Fields

        #region DEBUG
        private BattleDebugLogger _battleDebugLogger;
        private const string DebugLogLaunchSequence = "Launch sequence: ";
        private GridManager _gridManager; //only needed for logging grid position
        #endregion DEBUG

        #region Private - Methods

        private void Start()
        {
            // get components
            _audioSource = GetComponent<AudioSource>();

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // debug
            _battleDebugLogger = new BattleDebugLogger(this);
            _gridManager = Context.GetGridManager;
        }

        #region Private - Methods - Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _teams = new Team[2];

            _teams[TeamAlpha] = new() { TeamNumber = BattleTeamNumber.TeamAlpha };
            foreach (IReadOnlyBattlePlayer player in data.TeamAlpha.Players) _teams[TeamAlpha].List.Add(player.PlayerShieldManager.transform);

            _teams[TeamBeta] = new() { TeamNumber = BattleTeamNumber.TeamBeta };
            foreach (IReadOnlyBattlePlayer player in data.TeamBeta.Players) _teams[TeamBeta].List.Add(player.PlayerShieldManager.transform);

            this.Publish(new SlingControllerReady());
        }
        #endregion Private - Methods - Message Listeners

        #region Private - Methods - Update

        private void Update()
        {
            if (!_slingActive) return;
            SlingUpdate();
            SlingIndicatorUpdate();
        }

        private void SlingUpdate()
        {
            _currentTeam.Distance = -1;

            if (_currentTeam.List.Count != 2) return;

            float player0YDistance = Mathf.Abs(_currentTeam.List[0].position.y);
            float player1YDistance = Mathf.Abs(_currentTeam.List[1].position.y);

            if (player0YDistance == player1YDistance) return;

            if (player0YDistance < player1YDistance)
            {
                _currentTeam.FrontPlayer = _currentTeam.List[0];
                _currentTeam.BackPlayer = _currentTeam.List[1];
            }
            else
            {
                _currentTeam.FrontPlayer = _currentTeam.List[1];
                _currentTeam.BackPlayer = _currentTeam.List[0];
            }

            Vector3 launchVector = _currentTeam.FrontPlayer.position - _currentTeam.BackPlayer.position;
            _currentTeam.Distance = launchVector.magnitude;
            _currentTeam.LaunchDirection = launchVector / _currentTeam.Distance;
        }

        private void SlingIndicatorUpdate()
        {
            if (_currentTeam.Distance >= 0)
            {
                float length = _currentTeam.Distance;
                _slingIndicator.SetPosition(_currentTeam.BackPlayer.position + (_currentTeam.LaunchDirection * (length * 0.5f)));
                _slingIndicator.SetRotationRadians(Mathf.Atan2(_currentTeam.LaunchDirection.y, _currentTeam.LaunchDirection.x));
                _slingIndicator.SetLength(length);
                //_slingIndicator.SetWingAngleDegrees(ClampAndRemap(_currentTeam.Distance, _minDistance, _maxDistance, _wingMinAngleDegrees, _wingMaxAngleDegrees));
                _slingIndicator.SetShow(true);
            }
            else
            {
                _slingIndicator.SetShow(false);
            }
        }

        #endregion Private - Methods - Update

        private void SlingLaunchPrivate(BattleTeamNumber teamNumber, Vector3 position, Vector3 direction, float speed)
        {
            _slingActive = false;
            _slingIndicator.SetShow(false);

            _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "launching ball (team: {0}, position: {1}, direction: {2}, speed {3})",
                teamNumber != BattleTeamNumber.NoTeam ? (teamNumber == BattleTeamNumber.TeamAlpha ? "team alpha" : "team beta") : "no team",
                position,
                direction,
                speed
            );
            _ball.Launch(position, direction, speed);
            _audioSource.Play();
            _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "finished");
            this.Publish(new BallSlinged(teamNumber));
        }

        private IEnumerator MovePusherCoroutine(float duration)
        {
            _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "animation started");
            float speed = 1 / duration;
            float pusherPosition = 0;
            while (pusherPosition < 1)
            {
                yield return null;
                pusherPosition += speed * Time.deltaTime;
                _slingIndicator.SetPusherPosition(pusherPosition);
            }
            _battleDebugLogger.LogInfo(DebugLogLaunchSequence + "animation finished");
        }

        #endregion Private - Methods

        #endregion Private
    }
}
