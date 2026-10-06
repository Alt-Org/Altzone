using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using MenuUi.Scripts.Lobby.SelectedCharacters;
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
    [SerializeField] private TextLanguageSelectorCaller _playerName;
    [SerializeField] private Image _masterClient;

    private int? _slotIndex = null;
    private string _playerId = null;

    private const string BOT_ID = "Bot";

    public delegate void SetPositon(int position);
    public static event SetPositon OnSetPositon;

    public delegate void SetBot(int position, bool value);
    public static event SetBot OnSetBot;

    public delegate void SetReady(int position, bool value);
    public static event SetReady OnSetReady;

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
        if (selectedCharacterIds == null)
        {
            _playerId = string.Empty;
            if(SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
                _playerName.SetText("Avoin paikka");
            else
                _playerName.SetText("Free slot");
            _playerName.GetComponent<TMP_Text>().color = Color.white;
            _slotButton.GetComponent<Image>().sprite = _freePlayerSprite;
            _botButton.GetComponent<Image>().sprite = _botSprite;
            _botButton.gameObject.SetActive(true);
            _readyToggle.gameObject.SetActive(false);
            _botButton.SetIsOnWithoutNotify(false);
            _masterClient.gameObject.SetActive(false);
            return;
        }

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
        _readyToggle.gameObject.SetActive(true);
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
        _playerId = BOT_ID;
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
            _playerName.SetText("Botti");
        else
            _playerName.SetText("Bot");
        _playerName.GetComponent<TMP_Text>().color = Color.white;
        _slotButton.GetComponent<Image>().sprite = _botSprite;
        _botButton.GetComponent<Image>().sprite = _freePlayerSprite;
        _botButton.gameObject.SetActive(true);
        _readyToggle.gameObject.SetActive(false);
        _botButton.SetIsOnWithoutNotify(true);
        _masterClient.gameObject.SetActive(false);
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
            OnSetReady?.Invoke((int)_slotIndex, value);
        }
    }
}
