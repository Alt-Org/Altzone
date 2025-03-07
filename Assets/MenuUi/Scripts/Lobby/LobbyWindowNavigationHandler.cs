using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;

public class LobbyWindowNavigationHandler : MonoBehaviour
{
    [SerializeField]
    private WindowNavigation _mainMenuNavigation;
    [SerializeField]
    private WindowNavigation _battleStartNavigation;
    [SerializeField]
    private WindowNavigation _battleNavigation;
    [SerializeField]
    private WindowNavigation _battleStoryNavigation;
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

    private void WindowNavigation(LobbyWindowTarget target, LobbyWindowTarget lobbyWindow)
    {
        if(!CheckWindowStatus(lobbyWindow)) return;

        switch (target)
        {
            case LobbyWindowTarget.MainMenu:
                StartCoroutine(_mainMenuNavigation.Navigate());
                break;
            case LobbyWindowTarget.LobbyRoom:
                OnLobbyWindowChangeRequest?.Invoke();
                //StartCoroutine(_lobbyRoomNavigation.Navigate());
                break;
            case LobbyWindowTarget.BattleLoad:
                StartCoroutine(_battleStartNavigation.Navigate());
                break;
            case LobbyWindowTarget.Battle:
                StartCoroutine(_battleNavigation.Navigate());
                break;
            case LobbyWindowTarget.BattleStory:
                StartCoroutine(_battleStoryNavigation.Navigate());
                break;
            case LobbyWindowTarget.Raid:
                StartCoroutine(_raidNavigation.Navigate());
                break;
           
        }
    }

    private bool CheckWindowStatus(LobbyWindowTarget windowToCheck)
    {
        WindowDef currentWindow = WindowManager.Get().CurrentWindow;
        WindowDef checkWindow = null;

        switch (windowToCheck)
        {
            case LobbyWindowTarget.MainMenu:
                checkWindow = _mainMenuNavigation.NaviTarget;
                break;
            case LobbyWindowTarget.LobbyRoom:
                checkWindow = _lobbyRoomNavigation.NaviTarget;
                break;
            case LobbyWindowTarget.BattleLoad:
                checkWindow = _battleStartNavigation.NaviTarget;
                break;
            case LobbyWindowTarget.Battle:
                checkWindow = _battleNavigation.NaviTarget;
                break;
            case LobbyWindowTarget.BattleStory:
                checkWindow = _battleStoryNavigation.NaviTarget;
                break;
            case LobbyWindowTarget.Raid:
                checkWindow = _raidNavigation.NaviTarget;
                break;
            default:
                return true;
        }

        return currentWindow.WindowName.Equals(checkWindow.WindowName);
    }
}
