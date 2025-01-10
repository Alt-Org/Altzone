using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.GA;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Battle1.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Game
{
    #region Message Classes
    internal class GameStarted
    { }
    #endregion Message Classes

    internal class GameController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private float[] _gameStageChangeTimesSec;
        #endregion

        #region Private Constants
        // Game Startup
        private const double GameStartDelay = 1.0;
        #endregion Private Constants

        #region Private Fields

        // Game Startup
        private bool _syncedFixedUpdateClockStarted = false;
        private bool _teamsAreReadyForGameplay = false;
        private bool _slingControllerReady = false;
        private int _clientsReady = 0;
        private BallHandler _ballHandler;

        // State
        private bool _gameStarted = false;
        private int _gameStage = 0;

        private int _gameStartUpdateNumber;

        // Components
       /* private PhotonView _photonView;*/

        // Other Control Objects
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;
        private PlayerManager _playerManager;
        private SlingController _slingController;

        #endregion Private Fields

        #region DEBUG
        private BattleDebugLogger _battleDebugLogger;
        private const string DebugLogGameStartup = "GAME STARTUP: ";
        private const string DebugLogSlingSequence = "Sling sequence: ";
        #endregion DEBUG

        #region Private Methods

        private void Start()
        {
            // get components
           /* _photonView = GetComponent<PhotonView>();*/

            // get other control objects
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _playerManager = Context.GetPlayerManager;
            _slingController = Context.GetSlingController;
            _ballHandler = Context.GetBallHandler;

            BattleDebugLogger.Init(_syncedFixedUpdateClock);

            // subscribe to messages
            this.Subscribe<SyncedFixedUpdateClockStarted>(OnSyncedFixedUpdateClockStarted);
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
            this.Subscribe<SlingControllerReady>(OnSlingControllerReady);
            this.Subscribe<BallSlinged>(OnBallSlinged);
            this.Subscribe<SoulWallSegmentRemoved>(OnSoulWallSegmentRemoved);

            // debug
            _battleDebugLogger = new(this);

            // debug test
            _battleDebugLogger.LogInfo("test");
        }

        private void OnDestroy()
        {
            BattleDebugLogger.End();
        }

        #region Private Methods - Message Listeners

        #region Private Methods - Message Listeners - Game Startup

        private void OnSyncedFixedUpdateClockStarted(SyncedFixedUpdateClockStarted data)
        {
            _battleDebugLogger.LogInfo(DebugLogGameStartup + "SYNCED FIXED UPDATE CLOCK STARTED");
            _syncedFixedUpdateClockStarted = true;
            TryToStart();
        }

        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _battleDebugLogger.LogInfo(DebugLogGameStartup + "TEAMS ARE READY FOR GAMEPLAY");
            _teamsAreReadyForGameplay = true;
            TryToStart();
        }

        private void OnSlingControllerReady(SlingControllerReady data)
        {
            _battleDebugLogger.LogInfo(DebugLogGameStartup + "SLING CONTROLLER READY");
            _slingControllerReady = true;
            TryToStart();
        }

        #endregion Message Listeners - Game Startup

        private void OnBallSlinged(BallSlinged data)
        {
            _battleDebugLogger.LogInfo("Ball slinged");
            _battleDebugLogger.LogInfo(DebugLogSlingSequence + "enabling player movement");
            _playerManager.SetPlayerMovementEnabled(true);
            _battleDebugLogger.LogInfo(DebugLogSlingSequence + "finished");
        }

        private void OnSoulWallSegmentRemoved(SoulWallSegmentRemoved data)
        {
            _battleDebugLogger.LogInfo("Brick removed");
       /*     if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SlingReactivate), RpcTarget.All, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay), data.Side);
            }*/
        }

        #endregion Private Methods - Message Listeners

        #region Private Methods - Game Startup
        private void TryToStart()
        {
            if (!_teamsAreReadyForGameplay) return;
            if (!_syncedFixedUpdateClockStarted) return;
            if (!_slingControllerReady) return;

           /* _photonView.RPC(nameof(TryToStartGameRpc), RpcTarget.All);*/
        }
        #endregion Private Methods - Game Startup

        private void StageChange()
        {
            _gameStage++;
            _battleDebugLogger.LogInfo("_gameStage {0}", _gameStage);

            _ballHandler.BallSpeedup();

            ScheduleNextStageChange();
        }

        private void ScheduleNextStageChange()
        {
            if (_gameStage < _gameStageChangeTimesSec.Length)
            {
                int nextStageUpdate = _gameStartUpdateNumber + _syncedFixedUpdateClock.ToUpdates(_gameStageChangeTimesSec[_gameStage]);
                _battleDebugLogger.LogInfo("nextStageUpdate {0}", nextStageUpdate);
                _syncedFixedUpdateClock.ExecuteOnUpdate(nextStageUpdate, 0, StageChange);
            }
        }

        private void Sling(BattleTeamNumber slingingTeam)
        {
            _battleDebugLogger.LogInfo(DebugLogSlingSequence + "activating sling");
            _slingController.SlingActivate(slingingTeam);
            int slingModeEndUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._slingAimingTimeSec);

            _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber, 0, () =>
            {
                _battleDebugLogger.LogInfo(DebugLogSlingSequence + "disabling player movement");
                _playerManager.SetPlayerMovementEnabled(false);
            });

            _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay), 0, () =>
            {
                _battleDebugLogger.LogInfo(DebugLogSlingSequence + "waiting for players to stop moving");
                _syncedFixedUpdateClock.ExecuteOnUpdate(Mathf.Max(_playerManager.LastPlayerTeleportUpdateNumber, _syncedFixedUpdateClock.UpdateCount) + 1, -1, () =>
                {
                    _battleDebugLogger.LogInfo(DebugLogSlingSequence + "launching sling");
                    _slingController.SlingLaunch();
                });
            });
        }

        #region Private Methods - Photon RPC

        #region Private Methods - Photon RPC - Game Startup

      /*  [PunRPC]
        private void TryToStartGameRpc()
        {
            _clientsReady++;
            int realPlayerCount = PhotonNetwork.CurrentRoom.Players.Values.Sum(x => PhotonBattle.IsRealPlayer(x) ? 1 : 0);
            _battleDebugLogger.LogInfo(DebugLogGameStartup + "{0}/{1} CLIENTS READY", _clientsReady, realPlayerCount);
            if (_clientsReady < realPlayerCount) return;

            if (PhotonNetwork.IsMasterClient)
            {
                BattleTeamNumber slingingTeam = Random.Range(0, 2) == 0 ? BattleTeamNumber.TeamAlpha : BattleTeamNumber.TeamBeta;
                _photonView.RPC(nameof(StartGameRpc), RpcTarget.All, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameStartDelay), slingingTeam);
                GameAnalyticsManager.Instance.BattleLaunch();
            }
        }*/

       /* [PunRPC]
        private void StartGameRpc(int gameStartUpdateNumber, BattleTeamNumber slingingTeam)
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(gameStartUpdateNumber, 0, () =>
            {
                _battleDebugLogger.LogInfo(DebugLogGameStartup + "STARTING GAME {0}", gameStartUpdateNumber);
                _gameStartUpdateNumber = gameStartUpdateNumber;

                *//*int nextStageUpdate = _gameStartUpdateNumber + _syncedFixedUpdateClock.ToUpdates(_gameStageChangeTimesSec[_gameStage]);
                _syncedFixedUpdateClock.ExecuteOnUpdate(nextStageUpdate, 0, ScheduleNextStageChange);*//*
                ScheduleNextStageChange();

                _battleDebugLogger.LogInfo(DebugLogGameStartup + "ENABLING PLAYER MOVEMENT");
                _playerManager.SetPlayerMovementEnabled(true);
                _battleDebugLogger.LogInfo(DebugLogGameStartup + "STARTING SLING SEQUENCE");
                Sling(slingingTeam);
                _gameStarted = true;
                _battleDebugLogger.LogInfo(DebugLogGameStartup + "GAME STARTED");
                this.Publish(new GameStarted());
            });
            _playerManager.AnalyticsReportPlayerCharacterSelection();
        }*/

        #endregion Private Methods - Photon RPC - Game Startup

       /* [PunRPC]
        private void SlingReactivate(int activationUpdateNumber, BattleTeamNumber slingingTeam)
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(activationUpdateNumber, 0, () =>
            {
                _battleDebugLogger.LogInfo("Starting sling sequence");
                Sling(slingingTeam);
            });
        }*/

        #endregion Private Methods - Photon RPC

        #endregion Private Methods
    }
}
