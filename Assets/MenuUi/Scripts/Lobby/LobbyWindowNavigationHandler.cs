using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Window;
using UnityEngine;

public class LobbyWindowNavigationHandler : MonoBehaviour
{
    [SerializeField]
    private WindowNavigation _mainMenuNavigation;
    [SerializeField]
    private WindowNavigation _battleNavigation;
    [SerializeField]
    private WindowNavigation _raidNavigation;
    [SerializeField]
    private WindowNavigation _lobbyRoomNavigation;

    public delegate void LobbyWindowChangeRequest();
    public static event LobbyWindowChangeRequest OnLobbyWindowChangeRequest;

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.OnLobbyWindowChangeRequest += WindowNavigation;
    }

    private void OnDestroy()
    {
        LobbyManager.OnLobbyWindowChangeRequest -= WindowNavigation;
    }

    private void WindowNavigation(LobbyWindowTarget target)
    {
        switch (target)
        {
            case LobbyWindowTarget.MainMenu:
                StartCoroutine(_mainMenuNavigation.Navigate());
                break;
            case LobbyWindowTarget.LobbyRoom:
                OnLobbyWindowChangeRequest?.Invoke();
                //StartCoroutine(_lobbyRoomNavigation.Navigate());
                break;
            case LobbyWindowTarget.Battle:
                StartCoroutine(_battleNavigation.Navigate());
                break;
            case LobbyWindowTarget.Raid:
                StartCoroutine(_raidNavigation.Navigate());
                break;
        }
    }
}
