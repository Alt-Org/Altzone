using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prg.Scripts.Common.PubSub;
using Photon.Pun;
using System.Linq;
using Battle.Scripts.Battle;

public class GameStarted
{ }

public class GameControllerTest : MonoBehaviour
{
    private bool _syncedFixedUpdateClockStarted = false;
    private bool _teamsAreReadyForGameplay = false;
    private bool _slingControllerReady = false;

    private int _clientsReady = 0;

    private bool _gameStarted = false;

    private PhotonView _photonView;

    private SlingControllerTest _slingController;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();

        _slingController = Context.GetSlingController;

        this.Subscribe<SyncedFixedUpdateClockStarted>(OnSyncedFixedUpdateClockStarted);
        this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        this.Subscribe<SlingControllerReady>(OnSlingControllerReady);
    }

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

    private void TryToStart()
    {
        if (!_teamsAreReadyForGameplay) return;
        if (!_syncedFixedUpdateClockStarted) return;
        if (!_slingControllerReady) return;

        _photonView.RPC(nameof(TryToStartRpc), RpcTarget.All);
    }

    [PunRPC]
    private void TryToStartRpc()
    {
        _clientsReady++;
        int realPlayerCount = PhotonNetwork.CurrentRoom.Players.Values.Sum(x => PhotonBattle.IsRealPlayer(x) ? 1 : 0);
        Debug.Log("[GAME CONTROLLER] " + _clientsReady + "/" + realPlayerCount +  " CLIENTS READY");
        if (_clientsReady < realPlayerCount) return;

        if (PhotonNetwork.IsMasterClient)
        {
            _slingController.SlingActivate();
        }

        Debug.Log("[GAME CONTROLLER] GAME STARTED");
        _gameStarted = true;
        this.Publish(new GameStarted());
    }

    void Update()
    {
        
    }
}
