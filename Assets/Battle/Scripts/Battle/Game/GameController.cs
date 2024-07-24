using System.Linq;
using UnityEngine;
using Photon.Pun;

using Altzone.Scripts.Config;
using Altzone.Scripts.GA;
using Prg.Scripts.Common.PubSub;

using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;

#region Message Classes
public class GameStarted
{ }
#endregion Message Classes

public class GameController : MonoBehaviour
{
    // Game Startup
    private const double GAME_START_DELAY = 1.0;
    private bool _syncedFixedUpdateClockStarted = false;
    private bool _teamsAreReadyForGameplay = false;
    private bool _slingControllerReady = false;
    private int _clientsReady = 0;

    // State
    private bool _gameStarted = false;

    // Components
    private PhotonView _photonView;

    // Other Control Objects
    private SyncedFixedUpdateClock _syncedFixedUpdateClock;
    private PlayerManager _playerManager;
    private SlingController _slingController;

    // Degbug
    private const string DEBUG_LOG_NAME = "[BATTLE] [GAME CONTROLLER] ";
    private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
    private const string DEBUG_LOG_GAME_STARTUP = DEBUG_LOG_NAME + "GAME STARTUP: ";
    private const string DEBUG_LOG_SLING_SEQUENCE = DEBUG_LOG_NAME_AND_TIME + "Sling sequence: ";

    private void Start()
    {
        // get components
        _photonView = GetComponent<PhotonView>();

        // get other control objects
        _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        _playerManager = Context.GetPlayerManager;
        _slingController = Context.GetSlingController;

        // subscribe to messages
        this.Subscribe<SyncedFixedUpdateClockStarted>(OnSyncedFixedUpdateClockStarted);
        this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        this.Subscribe<SlingControllerReady>(OnSlingControllerReady);
        this.Subscribe<BallSlinged>(OnBallSlinged);
        this.Subscribe<BrickRemoved>(OnBrickRemoved);
    }

    #region Message Listeners

    #region Message Listeners - Game Startup

    private void OnSyncedFixedUpdateClockStarted(SyncedFixedUpdateClockStarted data)
    {
        Debug.Log(DEBUG_LOG_GAME_STARTUP + "SYNCED FIXED UPDATE CLOCK STARTED");
        _syncedFixedUpdateClockStarted = true;
        TryToStart();
    }

    private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        Debug.Log(DEBUG_LOG_GAME_STARTUP + "TEAMS ARE READY FOR GAMEPLAY");
        _teamsAreReadyForGameplay = true;
        TryToStart();
    }

    private void OnSlingControllerReady(SlingControllerReady data)
    {
        Debug.Log(DEBUG_LOG_GAME_STARTUP + "SLING CONTROLLER READY");
        _slingControllerReady = true;
        TryToStart();
    }

    #endregion Message Listeners - Game Startup

    private void OnBallSlinged(BallSlinged data)
    {
        Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Ball slinged", _syncedFixedUpdateClock.UpdateCount));
        Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "enabling player movement", _syncedFixedUpdateClock.UpdateCount));
        _playerManager.SetPlayerMovementEnabled(true);
        Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "finished", _syncedFixedUpdateClock.UpdateCount));
    }

    private void OnBrickRemoved(BrickRemoved data)
    {
        Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Brick removed", _syncedFixedUpdateClock.UpdateCount));
        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(SlingReactivate), RpcTarget.All, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay), data.Side);
        }
    }

    #endregion Message Listeners

    #region Private Methods

    #region Private Methods - Game Startup
    private void TryToStart()
    {
        if (!_teamsAreReadyForGameplay) return;
        if (!_syncedFixedUpdateClockStarted) return;
        if (!_slingControllerReady) return;

        _photonView.RPC(nameof(TryToStartGameRpc), RpcTarget.All);
    }
    #endregion Private Methods - Game Startup

    private void Sling(int slingingTeam)
    {
        Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "activating sling", _syncedFixedUpdateClock.UpdateCount));
        _slingController.SlingActivate(slingingTeam);
        int slingModeEndUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._slingAimingTimeSec);
        _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber, 0, () =>
        {
            Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "disabling player movement", _syncedFixedUpdateClock.UpdateCount));
            _playerManager.SetPlayerMovementEnabled(false);
        });

        _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay), 0, () =>
        {
            Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "waiting for players to stop moving", _syncedFixedUpdateClock.UpdateCount));
            _syncedFixedUpdateClock.ExecuteOnUpdate(Mathf.Max(_playerManager.LastPlayerTeleportUpdateNumber, _syncedFixedUpdateClock.UpdateCount) + 1, -1, () =>
            {
                Debug.Log(string.Format(DEBUG_LOG_SLING_SEQUENCE + "launching sling", _syncedFixedUpdateClock.UpdateCount));
                _slingController.SlingLaunch();
            });
        });
    }

    #endregion Private Methods

    #region Photon RPC

    #region Photon RPC - Game Startup

    [PunRPC]
    private void TryToStartGameRpc()
    {
        _clientsReady++;
        int realPlayerCount = PhotonNetwork.CurrentRoom.Players.Values.Sum(x => PhotonBattle.IsRealPlayer(x) ? 1 : 0);
        Debug.Log(string.Format(DEBUG_LOG_GAME_STARTUP + "{0}/{1} CLIENTS READY",_clientsReady, realPlayerCount));
        if (_clientsReady < realPlayerCount) return;

        if (PhotonNetwork.IsMasterClient)
        {
            int slingingTeam = Random.Range(0, 2) == 0 ? PhotonBattle.TeamAlphaValue : PhotonBattle.TeamBetaValue;
            _photonView.RPC(nameof(StartGameRpc), RpcTarget.All, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GAME_START_DELAY), slingingTeam);
            GameAnalyticsManager.Instance.BattleLaunch();
        }
    }

    [PunRPC]
    private void StartGameRpc(int gameStartUpdateNumber, int slingingTeam)
    {
        _syncedFixedUpdateClock.ExecuteOnUpdate(gameStartUpdateNumber, 0, () =>
        {
            Debug.Log(DEBUG_LOG_GAME_STARTUP + "STARTING GAME");
            Debug.Log(DEBUG_LOG_GAME_STARTUP + "ENABLING PLAYER MOVEMENT");
            _playerManager.SetPlayerMovementEnabled(true);
            Debug.Log(DEBUG_LOG_GAME_STARTUP + "STARTING SLING SEQUENCE");
            Sling(slingingTeam);
            _gameStarted = true;
            Debug.Log(DEBUG_LOG_GAME_STARTUP + "GAME STARTED");
            this.Publish(new GameStarted());
        });
        _playerManager.AnalyticsReportPlayerCharacterSelection();
    }

    #endregion Photon RPC - Game Startup

    [PunRPC]
    private void SlingReactivate(int activationUpdateNumber, int slingingTeam)
    {
        _syncedFixedUpdateClock.ExecuteOnUpdate(activationUpdateNumber, 0, () => {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Starting sling sequence", _syncedFixedUpdateClock.UpdateCount));
            Sling(slingingTeam);
        });
    }

    #endregion Photon RPC
}
