using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _battleStartText;
    [SerializeField]
    private TextMeshProUGUI _timerText;
    [SerializeField]
    private Image _loadImage;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private AnimationClip _battleStartAnimation;
    [SerializeField]
    private Sprite _tableAboveSprite;
    private Coroutine _timerCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        // Subscriptions moved to OnEnable to avoid receiving events while this GameObject is inactive.
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.DisableJukeBox();
        AudioManager.Instance.StopMusic(); //This should have a short sfx clip playing while the battle starts.
        OverlayPanelCheck.Instance.ToggleOverlay(false);
        LobbyManager.OnStartTimeSet += StartTimer;
        LobbyManager.OnGameStartCancelled += OnGameStartCancelled;
        LobbyManager.NotifyBattleStartUiReady();
    }

    private void OnDisable()
    {
        AudioManager.Instance.StopMusic();
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        LobbyManager.OnStartTimeSet -= StartTimer;
        LobbyManager.OnGameStartCancelled -= OnGameStartCancelled;
    }
    private void StartTimer(long startTime)
    {
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(TimerStart(startTime));
    }
    void OnDestroy()
    {
        LobbyManager.OnStartTimeSet -= StartTimer;
        LobbyManager.OnGameStartCancelled -= OnGameStartCancelled;
    }

    private IEnumerator TimerStart(long startTime)
    {
        //_loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.8f);
        //_loadImage.transform.localScale = new Vector2(1f, 1f);
        //_battleStartText.gameObject.SetActive(false);
        float animationLength = PlayBattleEnd();

        yield return new WaitForSeconds(animationLength + 3);
        LobbyManager.Instance.IsStartFinished = true;
        _timerCoroutine = null;
    }

    private float PlayBattleEnd()
    {
        _animator.Play(_battleStartAnimation.name);
        return _battleStartAnimation.length;
    }

    private void OnGameStartCancelled()
    {
        // Stop any running start animation and move back to main menu for matchmaking
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        try
        {
            AudioManager.Instance.StopMusic();
        }
        catch { }

        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.IsStartFinished = true; // unblock lobby waiting coroutines
        }

        LobbyManager.RequestLobbyWindowChange(LobbyWindowTarget.MainMenu);
    }
}
