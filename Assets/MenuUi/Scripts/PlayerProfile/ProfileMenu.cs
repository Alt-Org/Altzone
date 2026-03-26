using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using System.IO;
using UnityEngine.EventSystems;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using System.Data;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;

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
    [SerializeField] private TextMeshProUGUI _playerClanNameText;
    [SerializeField] private TextMeshProUGUI _rolesErrorMessage;
    [SerializeField] private TextMeshProUGUI _TimePlayedText;
    [SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private TextMeshProUGUI _LosesText;
    [SerializeField] private TextMeshProUGUI _WinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;

    [Header("Selectors")]
    [SerializeField] private GameObject _answerOptionPrefab;
    [SerializeField] private CharacterResponseList _characterResponseList;
    [SerializeField] private Image _favoriteCharacterImage;
    [SerializeField] private GameObject _characterOptionsPopup;
    [SerializeField] private Transform _characterOptionsContent;
    [SerializeField] private GameObject _characterOptionPrefab;
    [SerializeField] private TextMeshProUGUI _characterSelectionMessage;

    [Header("Avatar")]
    [SerializeField] private AvatarLoader _avatarLoaderInfoPage;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoaderTabline;

    [Header("Buttons")]
    [SerializeField] private Button _openFavoriteDefenceSelection;
    [SerializeField] private GameObject _closePopupAreaButton;
    [SerializeField] private GameObject[] _playStyleButtons;
    [SerializeField] private Button _avatarPageTabButton;

    [Header("Clan Button")]
    [SerializeField] private Button _ClanURLButton;
    [SerializeField] private Button _openViewedPlayerClanButton;
    [SerializeField] private WindowDef _clanWindowDef;

    [Header("Add Friend Button")]
    [SerializeField] private Button _addFriendButton;

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

    public TextMeshProUGUI textMeshPro;

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

    private const string _DarkOrange = "#FF6A00";
    private const string _Orange = "#FFA100";

    private ServerPlayer _player;

    private const string SelectionMessageDefault = "Paina tästä valitaksesi...";
    private const string SelectionMessageDefaultOther = "Ei valittu";

    private string _tempFavoriteDefenceID;

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
        if (minuteCount < 1)
        {
            _TimePlayedText.text = $"Alle 1 min";
        }
        else
        {
            _TimePlayedText.text = $"{minuteCount} min";
        }

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
        Debug.Log($"_ClanURLButton is null: {_ClanURLButton == null}");
        LoadMinutes();

        ServerManager.OnLogInStatusChanged += SetPlayerProfileValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerProfileValues(false);
        else
            SetPlayerProfileValues(true);

        if (!_otherPlayerProfile)
        {
            AddAnswerOptions();
        }
    }
    private void OnDisable()
    {
        _characterOptionsPopup.SetActive(false);
        _closePopupAreaButton.SetActive(false);
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
        _TimePlayedText.text = loggedOutTimeText;
        _LosesText.text = loggedOutLosesText;
        _WinsText.text = loggedOutWinsText;
        _CarbonText.text = loggedOutCarbonText;
    }

    /// <summary>
    /// Adds the answer options and opening/closing functionality to the selector popups
    /// </summary>
    private void AddAnswerOptions()
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
    }

    private void SaveChanges()
    {
        _playerData.FavoriteDefenceID = _tempFavoriteDefenceID;

        StartCoroutine(SavePlayerData(_playerData, null));
    }

    private void Start()
    {
        if (_ClanURLButton != null)
            _ClanURLButton.onClick.AddListener(OpenClanURL);

        if (_openViewedPlayerClanButton != null)
            _openViewedPlayerClanButton.onClick.AddListener(OpenViewedPlayerClanProfile);

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
        if (_playerData == null)
        {
            Debug.LogError("PlayerData is null in ProfileMenu.");
            return;
        }

        ToggleProfileViewMode();

        _playerName.text = _playerData.Name;
        _activityText.text = _playerData.points.ToString();

        if (_playerData.stats != null)
        {
            _WinsText.text = _playerData.stats.wonBattles.ToString();
        }

        PlayerCharacterPrototype favoriteDefence = PlayerCharacterPrototypes.GetCharacter(_playerData.FavoriteDefenceID);
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
        }

        if (_otherPlayerProfile)
        {
            _weekEmotions.ShowOtherPlayerEmotions();
        }
        else
        {
            _weekEmotions.ValuesToWeekEmotions(_playerData);
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
            _openFavoriteDefenceSelection.interactable = false; 
            foreach (GameObject button in _playStyleButtons)
            {
                button.SetActive(false);
            }
        }
        else
        {
            if (_openFavoriteDefenceSelection != null)
            {
                _openFavoriteDefenceSelection.interactable = true;
            }

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
