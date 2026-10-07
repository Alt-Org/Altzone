using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InRoomPlayerSlot : MonoBehaviour
{
    [SerializeField] private Sprite _botSprite;
    [SerializeField] private Sprite _freePlayerSprite;
    [SerializeField] private Button _slotButton;
    [SerializeField] private Toggle _botButton;
    [SerializeField] private Toggle _readyToggle;
    [SerializeField] private Color _readyColor;
    [SerializeField] private Color _unreadyColor;
    [SerializeField] private TextLanguageSelectorCaller _playerName;
    [SerializeField] private Image _masterClient;
    [SerializeField] private bool _customRoom;

    private int? _slotIndex = null;
    private string _playerId = "null";
    private bool _isOwn = false;

    private const string BOT_ID = "Bot";

    public string PlayerId { get => _playerId;}

    public delegate void SetPositon(int position);
    public static event SetPositon OnSetPositon;

    public delegate void SetBot(int position, bool value);
    public static event SetBot OnSetBot;

    public delegate void SetReady(int position, bool value);
    public static event SetReady OnSetReady;

    public delegate void ActivateReadyPanel();
    public static event ActivateReadyPanel OnActivateReadyPanel;

    private void Start()
    {
        _botButton.onValueChanged.AddListener(ToggleBot);
        _slotButton.onClick.AddListener(SetPosition);
        _readyToggle.onValueChanged.AddListener(ToggleReady);
    }

    private void OnDestroy()
    {
        _botButton.onValueChanged.RemoveAllListeners();
        _slotButton.onClick.RemoveAllListeners();
        _readyToggle.onValueChanged.RemoveAllListeners();
    }

    public void Initialize(int index)
    {
        _slotIndex = index;
    }

    /// <summary>
    /// Set player characters based on given selected character ids. Stats are passed onwards to initialize piechart preview.
    /// </summary>
    /// <param name="selectedCharacterIds">The selected character ids to display.</param>
    /// <param name="stats">The stats for all three characters in an int array. Order: Hp, Speed, CharacterSize, Attack, Defence.</param>
    public void SetCharacters(string playerId, string playerName, int[] selectedCharacterIds, bool ownSlot, bool isMasterClient)
    {
        if (_playerId == playerId) return;
        if (selectedCharacterIds == null)
        {
            _playerId = string.Empty;
            if(SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
                _playerName.SetText("Avoin paikka");
            else
                _playerName.SetText("Free slot");
            _playerName.GetComponent<TMP_Text>().color = Color.white;
            _isOwn = false;
            _slotButton.GetComponent<Image>().sprite = _freePlayerSprite;
            _botButton.GetComponent<Image>().sprite = _botSprite;
            if(_customRoom) _botButton.gameObject.SetActive(true);
            else _botButton.gameObject.SetActive(false);
            _readyToggle.gameObject.SetActive(false);
            _readyToggle.SetIsOnWithoutNotify(false);
            _readyToggle.GetComponent<Image>().color = _unreadyColor;
            _botButton.SetIsOnWithoutNotify(false);
            _masterClient.gameObject.SetActive(false);
            return;
        }

        _isOwn = ownSlot;
        _playerId = playerId;
        _playerName.SetText(playerName);
        if (ownSlot)
        {
            _playerName.GetComponent<TMP_Text>().color = Color.blue;
        }
        else
        {
            _playerName.GetComponent<TMP_Text>().color = Color.white;
        }
        if (_customRoom) _readyToggle.gameObject.SetActive(true);
        _botButton.SetIsOnWithoutNotify(false);
        _masterClient.gameObject.SetActive(isMasterClient);

        if (selectedCharacterIds[0] == (int)CharacterID.None)
        {
            _slotButton.GetComponent<Image>().sprite = _freePlayerSprite;
            _botButton.GetComponent<Image>().sprite = _botSprite;
            _botButton.gameObject.SetActive(true);
            return;
        }

        PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(selectedCharacterIds[0].ToString());

        _slotButton.GetComponent<Image>().sprite = charInfo.GalleryImage;
        _botButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set slots to teh bot mode where there is just a blank head in the slot if no characters are given.
    /// </summary>
    /// <param name="selectedCharacterIds">The selected character ids to display.</param>
    public void SetBotCharacters()
    {
        if (_playerId == BOT_ID) return;
        _playerId = BOT_ID;
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
            _playerName.SetText("Botti");
        else
            _playerName.SetText("Bot");
        _playerName.GetComponent<TMP_Text>().color = Color.white;
        _isOwn = false;
        _slotButton.GetComponent<Image>().sprite = _botSprite;
        _botButton.GetComponent<Image>().sprite = _freePlayerSprite;
        _botButton.gameObject.SetActive(true);
        _readyToggle.gameObject.SetActive(false);
        _readyToggle.SetIsOnWithoutNotify(false);
        _readyToggle.GetComponent<Image>().color = _unreadyColor;
        _botButton.SetIsOnWithoutNotify(true);
        _masterClient.gameObject.SetActive(false);
    }

    public void SetReadyState(bool value)
    {
        bool oldValue = _readyToggle.isOn;
        Debug.LogWarning(oldValue);
        _readyToggle.SetIsOnWithoutNotify(value);
        _readyToggle.GetComponent<Image>().color = value ? _readyColor: _unreadyColor;
        Debug.LogWarning(oldValue);
        if (oldValue == value) return;
        if (_isOwn && value)
        {
            OnActivateReadyPanel?.Invoke();
        }
    }

    private void ToggleBot(bool value)
    {
        if (!_slotIndex.HasValue)
        {
            Debug.LogError("Slot has no position value. Unable to set bot to position.");
            return;
        }
        if (string.IsNullOrEmpty(_playerId) || _playerId == BOT_ID)
        {
            OnSetBot?.Invoke((int)_slotIndex, value);
        }
    }

    private void SetPosition()
    {
        if (!_slotIndex.HasValue)
        {
            Debug.LogError("Slot has no position value. Unable to set player to position.");
            return;
        }
        if (string.IsNullOrEmpty(_playerId))
        {
            OnSetPositon?.Invoke((int)_slotIndex);
        }
    }

    private void ToggleReady(bool value)
    {
        if (!_slotIndex.HasValue)
        {
            Debug.LogError("Slot has no position value. Unable to set player to position.");
            return;
        }
        if (!string.IsNullOrEmpty(_playerId))
        {
            _readyToggle.SetIsOnWithoutNotify(!value);
            OnSetReady?.Invoke((int)_slotIndex, value);
        }
    }
}
