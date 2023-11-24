using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prg.Scripts.Common.PubSub;
using Photon.Pun;
using System.Linq;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Altzone.Scripts.Config;

#region Message Classes
public class GameStarted
{ }
#endregion Message Classes

public class GameControllerTest : MonoBehaviour
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
    private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;
    private PlayerManager _playerManager;
    private SlingController _slingController;

    void Start()
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
        Debug.Log("[GAME CONTROLLER] SYNCED FIXED UPDATE CLOCK STARTED");
        _syncedFixedUpdateClockStarted = true;
        TryToStart();
    }

    private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        Debug.Log("[GAME CONTROLLER] TEAMS ARE READY FOR GAMEPLAY");
        _teamsAreReadyForGameplay = true;
        TryToStart();
    }

    private void OnSlingControllerReady(SlingControllerReady data)
    {
        Debug.Log("[GAME CONTROLLER] SLING CONTROLLER READY");
        _slingControllerReady = true;
        TryToStart();
    }

    #endregion Message Listeners - Game Startup

    private void OnBallSlinged(BallSlinged data)
    {
        _playerManager.SetPlayerMovementEnabled(true);
    }

    private void OnBrickRemoved(BrickRemoved data)
    {
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
        _slingController.SlingActivate(slingingTeam);
        int slingModeEndUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._slingAimingTimeSec);
        _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber, 0, () =>
        {
            _playerManager.SetPlayerMovementEnabled(false);
        });

        _syncedFixedUpdateClock.ExecuteOnUpdate(slingModeEndUpdateNumber + _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay), 0, () =>
        {
            _syncedFixedUpdateClock.ExecuteOnUpdate(Mathf.Max(_playerManager.LastPlayerTeleportUpdateNumber, _syncedFixedUpdateClock.UpdateCount) + 1, -1, () =>
            {
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
        Debug.Log("[GAME CONTROLLER] " + _clientsReady + "/" + realPlayerCount + " CLIENTS READY");
        if (_clientsReady < realPlayerCount) return;

        if (PhotonNetwork.IsMasterClient)
        {
            int slingingTeam = Random.Range(0, 2) == 0 ? PhotonBattle.TeamAlphaValue : PhotonBattle.TeamBetaValue;
            _photonView.RPC(nameof(StartGameRpc), RpcTarget.All, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(GAME_START_DELAY), slingingTeam);
        }
    }

    [PunRPC]
    private void StartGameRpc(int gameStartUpdateNumber, int slingingTeam)
    {
        _syncedFixedUpdateClock.ExecuteOnUpdate(gameStartUpdateNumber, 0, () =>
        {
            _playerManager.SetPlayerMovementEnabled(true);
            Sling(slingingTeam);
            _gameStarted = true;
            Debug.Log("[GAME CONTROLLER] GAME STARTED");
            this.Publish(new GameStarted());
        });
    }

    #endregion Photon RPC - Game Startup

    [PunRPC]
    private void SlingReactivate(int activationUpdateNumber, int slingingTeam)
    {
        _syncedFixedUpdateClock.ExecuteOnUpdate(activationUpdateNumber, 0, () => { Sling(slingingTeam); });
    }

    #endregion Photon RPC
}
