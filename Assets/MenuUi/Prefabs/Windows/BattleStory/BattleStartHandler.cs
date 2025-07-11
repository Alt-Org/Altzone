using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using Altzone.Scripts.Lobby.Wrappers;

public class BattleStartHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _battleStartText;
    [SerializeField]
    private TextMeshProUGUI _timerText;
    [SerializeField]
    private Image _loadImage;
    [SerializeField]
    private List<Sprite> _startAnimationSprites;
    [SerializeField]
    private float _animationFrameTime = 0.5f;
    [SerializeField]
    private Sprite _tableAboveSprite;

    [Header("Battle Players panel")]
    [SerializeField]
    private GameObject _battlePlayersPanel;
    [SerializeField]
    private BattlePopupCharacterSlotController[] _characterSlotControllers;
    [SerializeField]
    private TextMeshProUGUI[] _playerNames;

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.OnStartTimeSet += StartTimer;
    }

    private void OnEnable()
    {
        AudioManager.Instance.StopMusic(); //This should have a short sfx clip playing while the battle starts.
    }
    private void OnDisable()
    {
        AudioManager.Instance.StopMusic();
    }
    private void StartTimer(long startTime)
    {
        StartCoroutine(TimerStart(startTime));
    }
    void OnDestroy()
    {
        LobbyManager.OnStartTimeSet -= StartTimer;
    }

    private IEnumerator TimerStart(long startTime)
    {
        _loadImage.sprite = _startAnimationSprites[0];
        _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.8f);
        _loadImage.transform.localScale = new Vector2(1f, 1f);
        _battleStartText.gameObject.SetActive(false);
        //_timerText.gameObject.SetActive(false);
        float timeleft = startTime/1000f;
        float frametimeleft = 0;

        foreach (Sprite sprite in _startAnimationSprites)
        {
            frametimeleft += _animationFrameTime;
            _loadImage.sprite = sprite;
            do
            {
                yield return null;
                timeleft -= Time.deltaTime;
                frametimeleft -= Time.deltaTime;
            } while (frametimeleft > 0);
        }
        _battleStartText.gameObject.SetActive(true);
        //_timerText.gameObject.SetActive(true);
        do
        {
            //_timerText.text = Mathf.CeilToInt(timeleft).ToString();

            if (!_loadImage.sprite.Equals(_tableAboveSprite))
            {
                _loadImage.sprite = _tableAboveSprite;
                _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                _battlePlayersPanel.SetActive(true);
                SetPlayerInfos();
            }
            //float scale = 1f + 0.4f * (1 - (Mathf.Clamp(timeleft,0,3f) / 3f));
            //_loadImage.transform.localScale = new Vector2(scale, scale);

            yield return null;
            timeleft -= Time.deltaTime;
        } while (timeleft > 0f);
    }

    private void SetPlayerInfos()
    {
        if (PhotonRealtimeClient.LobbyCurrentRoom == null) return;
        
        foreach (LobbyPlayer player in PhotonRealtimeClient.GetCurrentRoomPlayers())
        {
            int playerPos = PhotonLobbyRoom.GetPlayerPos(player);
            if (!PhotonLobbyRoom.IsValidPlayerPos(playerPos)) continue;

            _playerNames[playerPos - 1].text = player.NickName;
            int[] characters = player.GetCustomProperty(PhotonLobbyRoom.PlayerPrefabIdsKey, new int[3]);
            _characterSlotControllers[playerPos - 1].SetCharacters(characters);
            _characterSlotControllers[playerPos - 1].gameObject.SetActive(true);
        }
    }
}
