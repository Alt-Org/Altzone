using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Common;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Window;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileMenu : AltMonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;
    [SerializeField] private string _loggedOutplayerClanNameText;
    [SerializeField] private string loggedOutTimeText;
    [SerializeField] private string loggedOutLosesText;
    [SerializeField] private string loggedOutWinsText;
    [SerializeField] private string loggedOutCarbonText;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _playerPlayStyleText;
    [SerializeField] private TextMeshProUGUI _playerClanNameText;
    [SerializeField] private TextMeshProUGUI _rolesErrorMessage;
    //[SerializeField] private TextMeshProUGUI _TimePlayedText;
    //[SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private TextMeshProUGUI _LosesText;
    [SerializeField] private TextMeshProUGUI _WinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;

    [Header("Selectors")]
    /*[SerializeField] private GameObject _answerOptionPrefab;
    [SerializeField] private CharacterResponseList _characterResponseList;
    [SerializeField] private Image _favoriteCharacterImage;
    [SerializeField] private GameObject _characterOptionsPopup;
    [SerializeField] private Transform _characterOptionsContent;
    [SerializeField] private GameObject _characterOptionPrefab;
    [SerializeField] private TextMeshProUGUI _characterSelectionMessage;*/

    [Header("Favorite Defence")]
    [SerializeField] private TextMeshProUGUI _favoriteDefenceWins;
    [SerializeField] private TextMeshProUGUI _favoriteDefenceBattles;

    [Header("Avatar")]
    [SerializeField] private AvatarLoader _avatarLoaderInfoPage;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoaderTabline;

    [Header("Buttons")]
    //[SerializeField] private Button _openFavoriteDefenceSelection;
    //[SerializeField] private GameObject _closePopupAreaButton;
    [SerializeField] private GameObject _popupOverlay;
    [SerializeField] private GameObject[] _playStyleButtons;
    [SerializeField] private Button _avatarPageTabButton;

    [Header("Clan Button")]
    [SerializeField] private Button _ClanURLButton;
    [SerializeField] private Button _openViewedPlayerClanButton;
    [SerializeField] private WindowDef _clanWindowDef;

    [Header("Friend Request Popup")]
    [SerializeField] private Button _addFriendButton;
    [SerializeField] private GameObject _friendRequestPopup;
    [SerializeField] private Button _closeFriendRequestPopupButton;
    [SerializeField] private TextMeshProUGUI _friendRequestPopupTitleText;

    [Header("Clan Heart")]
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Color _defaultClanHeartColor = Color.white;

    [Header("Role UI")]
    [SerializeField] private Image _playerRoleIconImage;
    [SerializeField] private ClanRoleCatalog _roleCatalog;
    [SerializeField] private Sprite _fallbackRoleIcon;

    [Header("Others")]
    [SerializeField] private PlayStyle _playStyle;
    [SerializeField] private WeekEmotions _weekEmotions;

    [Header("Emotions")]
    [SerializeField] private Image _todaysEmotionImage;
    [SerializeField] private Sprite _blankEmotionSprite;
    [SerializeField] private Sprite _joyEmotionSprite;
    [SerializeField] private Sprite _loveEmotionSprite;
    [SerializeField] private Sprite _playfulEmotionSprite;
    [SerializeField] private Sprite _sadEmotionSprite;
    [SerializeField] private Sprite _angryEmotionSprite;

    [Header("Own Profile Only UI")]
    [SerializeField] private GameObject _editProfileButton;
    [SerializeField] private GameObject _editProfilePopup;
    [SerializeField] private TMP_InputField _editNameInputField;
    [SerializeField] private TextMeshProUGUI _editNameErrorText;
    [SerializeField] private Button _cancelEditProfileButton;
    [SerializeField] private Button _confirmEditProfileButton;
    [SerializeField] private GameObject _todayEmotionSection;
    [SerializeField] private GameObject _weekEmotionsSection;

    [Header("Carbon Popup")]
    [SerializeField] private Button _openCarbonPopupButton;
    [SerializeField] private GameObject _carbonEmissionPopup;
    [SerializeField] private Button _closeCarbonPopupButton;

    public TextMeshProUGUI textMeshPro;

    private string _tempPlayerName;
    private int _tempPlayStyleIndex;

    private int tempLocalSaveTime;
    private float tempLocalSaveSecondsTime;
    private float tempLocalSaveToCarbon;
    private float tempLocalSaveCarbon;

    private int minuteCount;
    private float secondsCount;
    private float countToCarbon;
    private float carbonCount;

    private static string _clanID = string.Empty;
    private string _url = "https://altzone.fi/clans/" + _clanID;

    private float kgCarbon => CarbonFootprint.CarbonCount / 1000f;

    private PlayerData _playerData = null;
    private ClanData _clanData = null;

    //private const string _DarkOrange = "#FF6A00";
    //private const string _Orange = "#FFA100";

    private ServerPlayer _player;

    //private const string SelectionMessageDefault = "Paina tästä valitaksesi...";
    //private const string SelectionMessageDefaultOther = "Ei valittu";

    //private string _tempFavoriteDefenceID;

    private bool _otherPlayerProfile = false;

    private void Update()
    {
        updateTime();
    }

    private void updateTime()
    {
        secondsCount += Time.deltaTime;
        countToCarbon += Time.deltaTime;

        // Peliaika
        /*if (minuteCount < 1)
        {
            _TimePlayedText.text = $"Alle 1 min";
        }
        else
        {
            _TimePlayedText.text = $"{minuteCount} min";
        }*/

        // Hiilijalanjäljen näyttäminen g tai kg
        float carbonDisplay = CarbonFootprint.CarbonCount;
        string carbonUnit = "g";

        if (carbonDisplay >= 1000f)
        {
            carbonDisplay /= 1000f;
            carbonUnit = "kg";
        }

        // Hiilijananjälki teksti
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
        {
            _CarbonText.text = $"Hiilijalanjälki\n{carbonDisplay:F1}{carbonUnit}/CO2";
        }
        else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _CarbonText.text = $"Carbon Footprint\n{carbonDisplay:F1}{carbonUnit}/CO2";
        }

        // Päivittää peliajan
        if (secondsCount >= 60f)
        {
            minuteCount++;
            SaveMinutes();
            secondsCount = 0;
        }
    }

    private void OnEnable()
    {
        SetEditPopupState(false);
        SetCarbonPopupState(false);
        SetFriendRequestPopupState(false);

        Debug.Log($"_ClanURLButton is null: {_ClanURLButton == null}");
        LoadMinutes();

        ServerManager.OnLogInStatusChanged += SetPlayerProfileValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerProfileValues(false);
        else
            SetPlayerProfileValues(true);

        /*if (!_otherPlayerProfile)
        {
            AddAnswerOptions();
        }*/
    }
    private void OnDisable()
    {
        SetEditPopupState(false);
        SetCarbonPopupState(false);
        SetFriendRequestPopupState(false);

        //_characterOptionsPopup.SetActive(false);
        //_closePopupAreaButton.SetActive(false);
        ServerManager.OnLogInStatusChanged -= SetPlayerProfileValues;

        if (_otherPlayerProfile)
        {
            DataCarrier.GetData<PlayerData>(DataCarrier.PlayerProfile, clear: true, suppressWarning: true);
        }
    }

    private void Reset()
    {
        _playerName.text = loggedOutPlayerText;
        _playerClanNameText.text = _loggedOutplayerClanNameText;
        //_TimePlayedText.text = loggedOutTimeText;
        _LosesText.text = loggedOutLosesText;
        _WinsText.text = loggedOutWinsText;
        _CarbonText.text = loggedOutCarbonText;
    }

    /// <summary>
    /// Adds the answer options and opening/closing functionality to the selector popups
    /// </summary>
    /*private void AddAnswerOptions()
    {

        _closePopupAreaButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            _characterOptionsPopup.SetActive(false);
            _closePopupAreaButton.SetActive(false);
        });

        _openFavoriteDefenceSelection.onClick.AddListener(() =>
        {
            _characterOptionsPopup.SetActive(true);
            _closePopupAreaButton.SetActive(true);
        });

        // Get all defences for choosing the favorite
        IEnumerable<PlayerCharacterPrototype> characters = PlayerCharacterPrototypes.Prototypes.Where(c => c != null);
        foreach (PlayerCharacterPrototype character in characters)
        {
            GameObject defenceOption = Instantiate(_characterOptionPrefab, _characterOptionsContent);
            Button button = defenceOption.GetComponent<FavoriteDefenceOptionHandler>().SetData(character.Name, character.GalleryImage);
            button.onClick.AddListener(() =>
            {
                _tempFavoriteDefenceID = character.Id;
                _favoriteCharacterImage.sprite = character.GalleryImage;

                //Show image
                Image image = _favoriteCharacterImage;
                var tempColor = image.color;
                tempColor.a = 1f;
                image.color = tempColor;
                _characterSelectionMessage.text = "";

                _characterOptionsPopup.SetActive(false);
                _closePopupAreaButton.SetActive(false);
            });
        }
    }*/

    private void SaveChanges()
    {
        if (_playerData == null)
            return;

        //_playerData.FavoriteDefenceID = _tempFavoriteDefenceID;

        if (_editNameInputField != null && !_otherPlayerProfile)
        {
            string trimmedName = _editNameInputField.text?.Trim();
            if (!string.IsNullOrEmpty(trimmedName))
            {
                _playerData.Name = trimmedName;
            }
        }

        StartCoroutine(SavePlayerData(_playerData, null));
    }

    private void Start()
    {
        if (_ClanURLButton != null)
            _ClanURLButton.onClick.AddListener(OpenClanURL);

        if (_openViewedPlayerClanButton != null)
            _openViewedPlayerClanButton.onClick.AddListener(OpenViewedPlayerClanProfile);

        if (_editProfileButton != null)
        {
            Button editButton = _editProfileButton.GetComponent<Button>();
            if (editButton != null)
                editButton.onClick.AddListener(OpenEditProfilePopup);
        }

        if (_popupOverlay != null)
        {
            Button overlayButton = _popupOverlay.GetComponentInChildren<Button>();
            if (overlayButton != null)
                overlayButton.onClick.AddListener(CloseAllProfilePopups);
        }

        if (_cancelEditProfileButton != null)
            _cancelEditProfileButton.onClick.AddListener(CloseEditProfilePopup);

        if (_confirmEditProfileButton != null)
            _confirmEditProfileButton.onClick.AddListener(ConfirmEditProfileChanges);

        if (_openCarbonPopupButton != null)
            _openCarbonPopupButton.onClick.AddListener(OpenCarbonPopup);

        if (_closeCarbonPopupButton != null)
            _closeCarbonPopupButton.onClick.AddListener(CloseCarbonPopup);

        if (_addFriendButton != null)
            _addFriendButton.onClick.AddListener(OpenFriendRequestPopup);

        if (_closeFriendRequestPopupButton != null)
            _closeFriendRequestPopupButton.onClick.AddListener(CloseFriendRequestPopup);

        LoadMinutes();
    }

    // Avaa klaanin URL
    private void OpenClanURL()
    {
        if (_clanData != null && !string.IsNullOrEmpty(_clanID))
        {
            string url = "https://altzone.fi/clans/" + _clanID;
            Application.OpenURL(url);
            Debug.Log($"Avaa URL: {url}");
        }
        else
        {
            Debug.LogError("Klaanitiedot puuttuvat.");
        }
    }

    private void RefreshTodaysEmotionUI()
    {
        if (_todaysEmotionImage == null || _playerData == null)
            return;

        List<Emotion> emotions = _playerData.playerDataEmotionList;
        if (emotions == null || emotions.Count == 0)
        {
            _todaysEmotionImage.sprite = _blankEmotionSprite;
            return;
        }

        if (string.IsNullOrWhiteSpace(_playerData.emotionSelectorDate))
        {
            _todaysEmotionImage.sprite = _blankEmotionSprite;
            return;
        }

        if (!DateTime.TryParse(_playerData.emotionSelectorDate, out DateTime anchorDate))
        {
            _todaysEmotionImage.sprite = _blankEmotionSprite;
            return;
        }
        
        int dayOffset = (DateTime.Now.Date - anchorDate.Date).Days;

        if (dayOffset < 0 || dayOffset >= emotions.Count)
        {
            _todaysEmotionImage.sprite = _blankEmotionSprite;
            return;
        }

        Emotion todayEmotion = emotions[dayOffset];
        _todaysEmotionImage.sprite = GetEmotionSprite(todayEmotion);
    }

    private int GetCurrentWeekdayIndex()
    {
        return DateTime.Now.DayOfWeek switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => 0
        };
    }

    private Sprite GetEmotionSprite(Emotion emotion)
    {
        return emotion switch
        {
            Emotion.Joy => _joyEmotionSprite,
            Emotion.Love => _loveEmotionSprite,
            Emotion.Playful => _playfulEmotionSprite,
            Emotion.Sorrow => _sadEmotionSprite,
            Emotion.Anger => _angryEmotionSprite,
            _ => _blankEmotionSprite
        };
    }

    private void CloseAllProfilePopups()
    {
        CloseEditProfilePopup();
        CloseCarbonPopup();
        CloseFriendRequestPopup();
    }

    private void SetEditPopupState(bool isOpen)
    {
        if (_editProfilePopup != null)
            _editProfilePopup.SetActive(isOpen);

        if (_popupOverlay != null)
            _popupOverlay.SetActive(isOpen);
    }

    private void OpenEditProfilePopup()
    {
        if (_otherPlayerProfile)
            return;

        if (_editProfilePopup == null || _editNameInputField == null)
        {
            Debug.LogWarning("Edit profile popup references are missing.");
            return;
        }

        _tempPlayerName = _playerData != null ? _playerData.Name : string.Empty;
        _editNameInputField.text = _tempPlayerName;

        if (_playStyle != null && _playerData != null)
        {
            _tempPlayStyleIndex = (int)_playerData.playStyles;
            _playStyle.CurrentIndex = _tempPlayStyleIndex;
            _playStyle.RefreshUI();
        }

        if (_editNameErrorText != null)
            _editNameErrorText.text = "";

        SetEditPopupState(true);
    }

    private void CloseEditProfilePopup()
    {
        SetEditPopupState(false);

        if (_editNameInputField != null)
            _editNameInputField.text = _tempPlayerName;

        if (_playStyle != null)
        {
            _playStyle.CurrentIndex = _tempPlayStyleIndex;
            _playStyle.RefreshUI();
        }

        if (_editNameErrorText != null)
            _editNameErrorText.text = "";
    }

    private void ConfirmEditProfileChanges()
    {
        if (_otherPlayerProfile || _playerData == null || _editNameInputField == null)
            return;

        string newName = _editNameInputField.text?.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            if (_editNameErrorText != null)
                _editNameErrorText.text = "Nimi ei voi olla tyhjä.";
            return;
        }

        _playerData.Name = newName;
        _playerName.text = newName;

        if (_playStyle != null)
        {
            _playerData.playStyles = (PlayStyles)_playStyle.CurrentIndex;
            RefreshPlayerPlayStyleUI();
        }

        SaveChanges();

        if (_editNameErrorText != null)
            _editNameErrorText.text = "";

        SetEditPopupState(false);
    }
    private void SetCarbonPopupState(bool isOpen)
    {
        if (_carbonEmissionPopup != null)
            _carbonEmissionPopup.SetActive(isOpen);

        if (_popupOverlay != null)
            _popupOverlay.SetActive(isOpen);
    }
    private void OpenCarbonPopup()
    {
        SetCarbonPopupState(true);
    }

    private void CloseCarbonPopup()
    {
        SetCarbonPopupState(false);
    }

    private void SetFriendRequestPopupState(bool isOpen)
    {
        if (_friendRequestPopup != null)
            _friendRequestPopup.SetActive(isOpen);

        if (_popupOverlay != null)
            _popupOverlay.SetActive(isOpen);
    }

    private void RefreshFriendRequestPopupText()
    {
        if (_friendRequestPopupTitleText == null)
            return;

        string playerName = _playerData != null && !string.IsNullOrEmpty(_playerData.Name)
            ? _playerData.Name
            : "pelaajalle";

        _friendRequestPopupTitleText.text =
            $"Lähetä ystäväkutsu\npelaajalle <color=#FFFFFF>{playerName}</color>?";
    }

    private void OpenFriendRequestPopup()
    {
        if (!_otherPlayerProfile)
            return;

        RefreshFriendRequestPopupText();
        SetFriendRequestPopupState(true);
    }

    private void CloseFriendRequestPopup()
    {
        SetFriendRequestPopupState(false);
    }

    /// <summary>
    /// Sets the name of the Player and stats when log in status changes
    /// </summary>
    private void SetPlayerProfileValues(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            PlayerData player = DataCarrier.GetData<PlayerData>(DataCarrier.PlayerProfile, suppressWarning: true);
            var store = Storefront.Get();

            if (player != null)
            {
                _playerData = player;
                _otherPlayerProfile = true;
                _avatarPageTabButton.gameObject.SetActive(false);
                ApplyPlayerDataToUI();
            }
            else
            {
                _otherPlayerProfile = false;
                _avatarPageTabButton.gameObject.SetActive(true);

                store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p =>
                {
                    if (p == null)
                    {
                        Debug.LogError("Pelaajatietojen haku epäonnistui.");
                        return;
                    }

                    _playerData = p;
                    ApplyPlayerDataToUI();
                });

                return;
            }
        }
        else
        {
            Reset();
        }
    }

    private void ApplyPlayerDataToUI()
    {
        UpdateOwnProfileOnlyUI();

        if (_playerData == null)
        {
            Debug.LogError("PlayerData is null in ProfileMenu.");
            return;
        }

        ToggleProfileViewMode();

        _playerName.text = _playerData.Name;
        RefreshPlayerPlayStyleUI();
        //_activityText.text = _playerData.points.ToString();

        if (_playerData.stats != null)
        {
            int wins = _playerData.stats.wonBattles;
            int played = _playerData.stats.playedBattles;
            int losses = Mathf.Max(0, played - wins);

            _WinsText.text = wins.ToString();
            _LosesText.text = losses.ToString();
        }
        else
        {
            _WinsText.text = "0";
            _LosesText.text = "0";
        }

        /*PlayerCharacterPrototype favoriteDefence = PlayerCharacterPrototypes.GetCharacter(_playerData.FavoriteDefenceID);
        Image image = _favoriteCharacterImage;
        var tempColor = image.color;

        if (favoriteDefence != null)
        {
            tempColor.a = 1f;
            image.color = tempColor;
            _characterSelectionMessage.text = "";

            if (!_otherPlayerProfile)
            {
                _tempFavoriteDefenceID = _playerData.FavoriteDefenceID;
            }

            _favoriteCharacterImage.sprite = favoriteDefence.GalleryImage;
        }
        else
        {
            tempColor.a = 0f;
            image.color = tempColor;
            _characterSelectionMessage.text = _otherPlayerProfile ? SelectionMessageDefaultOther : SelectionMessageDefault;
        }*/

        if (_otherPlayerProfile)
        {
            _weekEmotions.ShowOtherPlayerEmotions();
        }
        else
        {
            _weekEmotions.ValuesToWeekEmotions(_playerData);
        }

        RefreshTodaysEmotionUI();

        if (_otherPlayerProfile)
        {
            _todaysEmotionImage.sprite = _blankEmotionSprite;
        }
        else
        {
            RefreshTodaysEmotionUI();
        }

        if (_playerData.SelectedCharacterId != 0)
        {
            AvatarVisualData avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(_playerData);
            _avatarLoaderInfoPage.UpdateVisuals(avatarVisualData);
            _avatarFaceLoaderTabline.UpdateVisuals(avatarVisualData);
        }

        var store = Storefront.Get();

        if (!string.IsNullOrEmpty(_playerData.ClanId))
        {
            store.GetClanData(_playerData.ClanId, clan =>
            {
                if (clan == null)
                {
                    if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
                    {
                        _rolesErrorMessage.text = "Klaania ei löydetty.";
                        Debug.LogError("Klaanitietojen haku epäonnistui.");
                    }
                    else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
                    {
                        _rolesErrorMessage.text = "Clan not found.";
                        Debug.LogError("Failed to fetch clan data.");
                    }

                    _playerClanNameText.text = "";
                    ResetClanHeartUI();
                    SetPlayerRoleUI(null);
                    return;
                }

                _clanData = clan;
                _playerClanNameText.text = _clanData.Name;
                RefreshClanHeartUI(_clanData);
                _clanID = _playerData.ClanId;
                _url = "https://altzone.fi/clans/" + _playerData.ClanId;

                RefreshPlayerRoleUI();
                _rolesErrorMessage.text = "";
            });
        }
        else
        {
            _clanData = null;
            _clanID = string.Empty;
            _playerClanNameText.text = "";
            ResetClanHeartUI();
            SetPlayerRoleUI(null);
        }

        updateTime();
    }

    /// <summary>
    /// Disables editing when viewing other player's profile
    /// </summary>
    private void ToggleProfileViewMode()
    {
        if (_otherPlayerProfile)
        {
            //_openFavoriteDefenceSelection.interactable = false; 
            foreach (GameObject button in _playStyleButtons)
            {
                button.SetActive(false);
            }
        }
        else
        {
            /*if (_openFavoriteDefenceSelection != null)
            {
                _openFavoriteDefenceSelection.interactable = true;
            }*/

            foreach (GameObject button in _playStyleButtons)
            {
                if (button != null)
                {
                    button.SetActive(true);
                } 
            }
        }
    }

    private void SaveMinutes()
    {
        PlayerPrefs.SetInt("Minutes", minuteCount);
        PlayerPrefs.Save();
        Debug.Log("Minutes saved.");
    }

    private void LoadMinutes()
    {
        if (PlayerPrefs.HasKey("Minutes"))
        {
            minuteCount = PlayerPrefs.GetInt("Minutes");
            Debug.Log($"Minutes successfully loaded: {minuteCount}");
        }
        else
        {
            Debug.Log("No saved minutes found.");
        }
    }

    private void UpdateOwnProfileOnlyUI()
    {
        bool isOwnProfile = !_otherPlayerProfile;

        if (_editProfileButton != null)
            _editProfileButton.SetActive(isOwnProfile);

        if (_todayEmotionSection != null)
            _todayEmotionSection.SetActive(isOwnProfile);

        if (_weekEmotionsSection != null)
            _weekEmotionsSection.SetActive(isOwnProfile);

        if (_addFriendButton != null)
            _addFriendButton.gameObject.SetActive(_otherPlayerProfile);
    }

    private void RefreshPlayerPlayStyleUI()
    {
        if (_playerPlayStyleText == null || _playerData == null)
            return;

        if (_playStyle != null)
        {
            string[] styles = SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _playStyle.englishStyles
                : _playStyle.finnishStyles;

            int index = (int)_playerData.playStyles;

            if (styles != null && index >= 0 && index < styles.Length)
            {
                _playerPlayStyleText.text = styles[index];
                return;
            }
        }

        _playerPlayStyleText.text = _playerData.playStyles.ToString().Replace("_", " ");
    }

    private void RefreshClanHeartUI(ClanData clanData)
    {
        if (_clanHeart == null)
            return;

        _clanHeart.SetOwnClanHeart = false;

        if (clanData == null)
        {
            _clanHeart.SetHeartColor(_defaultClanHeartColor);
            return;
        }

        _clanHeart.SetOtherClanColors(clanData);
    }

    private void ResetClanHeartUI()
    {
        if (_clanHeart == null)
            return;

        _clanHeart.SetOwnClanHeart = false;
        _clanHeart.SetHeartColor(_defaultClanHeartColor);
    }

    private void SetPlayerRoleUI(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            SetPlayerRoleIconVisible(false);
            return;
        }

        if (_playerRoleIconImage != null)
        {
            Sprite icon = _roleCatalog != null ? _roleCatalog.GetIcon(role) : null;
            if (icon == null) icon = _fallbackRoleIcon;

            if (icon != null)
            {
                _playerRoleIconImage.sprite = icon;
                SetPlayerRoleIconVisible(true);
            }
            else
            {
                SetPlayerRoleIconVisible(false);
            }
        }
    }

    private void SetPlayerRoleIconVisible(bool visible)
    {
        if (_playerRoleIconImage != null)
            _playerRoleIconImage.gameObject.SetActive(visible);
    }

    private void RefreshPlayerRoleUI()
    {
        if (string.IsNullOrEmpty(_playerData?.ClanId) || string.IsNullOrEmpty(_playerData?.Id))
        {
            SetPlayerRoleUI(null);
            return;
        }

        StartCoroutine(LoadViewedPlayerRoleCoroutine(_playerData.ClanId, _playerData.Id));
    }

    private IEnumerator LoadViewedPlayerRoleCoroutine(string clanId, string playerId)
    {
        List<ClanMember> members = null;

        yield return StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(clanId, m => members = m));

        if (members == null)
        {
            SetPlayerRoleUI(null);
            yield break;
        }

        ClanMember member = members.FirstOrDefault(m => m != null && m.Id == playerId);

        string roleName = "Member";

        if (member?.Role != null && !string.IsNullOrEmpty(member.Role.name))
        {
            roleName = member.Role.name;
        }

        SetPlayerRoleUI(roleName);
    }

    private void OpenViewedPlayerClanProfile()
    {
        if (_playerData == null || string.IsNullOrEmpty(_playerData.ClanId))
        {
            Debug.LogWarning("Viewed player has no clan.");
            return;
        }

        if (_clanWindowDef == null)
        {
            Debug.LogError("Clan window def is missing from ProfileMenu.");
            return;
        }

        StartCoroutine(ServerManager.Instance.GetClanFromServer(_playerData.ClanId, serverClan =>
        {
            if (serverClan == null)
            {
                Debug.LogError("Failed to fetch viewed player's clan from server.");
                return;
            }

            DataCarrier.GetData<ServerClan>(DataCarrier.ClanListing, clear: true, suppressWarning: true);
            DataCarrier.AddData(DataCarrier.ClanListing, serverClan);

            WindowManager.Get().ShowWindow(_clanWindowDef);
        }));
    }
}
