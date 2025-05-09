using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Scripts.Window;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.InputSystem.HID;
using System.IO;
using UnityEngine.EventSystems;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;

public class ProfileMenu : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string loggedOutPlayerText;
    [SerializeField] private string _loggedOutplayerClanNameText;
    [SerializeField] private string loggedOutTimeText;
    [SerializeField] private string loggedOutLosesText;
    [SerializeField] private string loggedOutWinsText;
    [SerializeField] private string loggedOutCarbonText;

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private TextMeshProUGUI _playerClanNameText;
    [SerializeField] private TextMeshProUGUI _rolesErrorMessage;
    [SerializeField] private TextMeshProUGUI _DefenceClassText;
    [SerializeField] private TextMeshProUGUI _MottoText;
    [SerializeField] private TextMeshProUGUI _TimePlayedText;
    [SerializeField] private TextMeshProUGUI _activityText;
    [SerializeField] private TextMeshProUGUI _LosesText;
    [SerializeField] private TextMeshProUGUI _WinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;
    //[SerializeField] private TMP_InputField _LifeQuoteInputField;
    [SerializeField] private TMP_InputField _LoreInputField;

    [Header("Selectors")]
    [SerializeField] private GameObject _answerOptionPrefab;
    [SerializeField] private GameObject _mottoOptionsPopup;
    [SerializeField] private CharacterResponseList _characterResponseList;
    [SerializeField] private Image _favoriteCharacterImage;
    [SerializeField] private GameObject _characterOptionsPopup;
    [SerializeField] private GameObject _characterOptionPrefab;

    [Header("Buttons")]
    [SerializeField] private Button _openMottoOptions;
    [SerializeField] private Button _openFavoriteDefenceSelection;
    [SerializeField] private GameObject _closePopupAreaButton;

    [Header("Clan Button")]
    [SerializeField] private Button _ClanURLButton;
    [SerializeField] private TextMeshProUGUI _ClanButtonText;

    [Header("Save Button")]
    [SerializeField] private Button _saveEditsButton;

    [Header("Add Friend Button")]
    [SerializeField] private Button _addFriendButton;

    [Header("Others")]

    [SerializeField] private PlayStyle _playStyle;

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

    private const string _mottoDefault = "Paina tästä valitaksesi...";

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

        _CarbonText.text = $"Hiilijalanjälki\n{carbonDisplay:F1}{carbonUnit}/CO2"; // Hiilijalanjälki teksti

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
        //Debug.Log($"Initial LifeQuote text: {_LifeQuoteInputField.text}");
        LoadInputFromFile();
        LoadMinutes();

        ServerManager.OnLogInStatusChanged += SetPlayerProfileValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerProfileValues(false);
        else
            SetPlayerProfileValues(true);


        AddAnswerOptions();
    }

    private void Reset()
    {
        _playerNameText.text = loggedOutPlayerText;
        _playerClanNameText.text = _loggedOutplayerClanNameText;
        _TimePlayedText.text = loggedOutTimeText;
        _LosesText.text = loggedOutLosesText;
        _WinsText.text = loggedOutWinsText;
        _CarbonText.text = loggedOutCarbonText;
    }

    // Tallentaa lifequote ja lore inputkentät
    public void SaveInputToFile()
    {
        //if (_LifeQuoteInputField != null)
        //{
        //    string lifeQuote = _LifeQuoteInputField.text;
        //    string path = Path.Combine(Application.persistentDataPath, "LifeQuote.txt");
        //    File.WriteAllText(path, lifeQuote);
        //    Debug.Log($"Saved Life Quote: {lifeQuote} at {path}");
        //}

        if (_LoreInputField != null)
        {
            string lore = _LoreInputField.text;
            string path = Path.Combine(Application.persistentDataPath, "Lore.txt");
            File.WriteAllText(path, lore);
            Debug.Log($"Saved Lore: {lore} at {path}");
        }

        if (_MottoText != null)
        {
            string motto = _MottoText.text;
            string path = Path.Combine(Application.persistentDataPath, "Motto.txt");
            File.WriteAllText(path, motto);
            Debug.Log($"Saved Motto: {motto} at {path}");
        }
    }

    // Lataa tallennetut tiedostot
    public void LoadInputFromFile()
    {
        string quotePath = Path.Combine(Application.persistentDataPath, "LifeQuote.txt");
        string lorePath = Path.Combine(Application.persistentDataPath, "Lore.txt");
        string mottoPath = Path.Combine(Application.persistentDataPath, "Motto.txt");
        if (File.Exists(quotePath))
        {
            //string loadedQuote = File.ReadAllText(quotePath);
            //Debug.Log($"Loaded Life Quote: {loadedQuote} from {quotePath}");
            //if (_LifeQuoteInputField != null)
            //{
            //    _LifeQuoteInputField.text = loadedQuote;
            //}
        }
        else
        {
            Debug.Log("No quote found.");
        }

        if (File.Exists(lorePath))
        {
            string loadedLore = File.ReadAllText(lorePath);
            Debug.Log($"Loaded Lore: {loadedLore} from {lorePath}");
            if (_LoreInputField != null)
            {
                _LoreInputField.text = loadedLore;
            }
        }
        else
        {
            Debug.Log("No lore found.");
        }

        if (File.Exists(mottoPath))
        {
            string loadedMotto = File.ReadAllText(mottoPath);
            Debug.Log($"Loaded Motto: {loadedMotto} from {mottoPath}");
            if (_MottoText != null)
            {
                _MottoText.text = loadedMotto;
            }
        }
        else
        {
            Debug.Log("No motto found.");
            _MottoText.text = _mottoDefault;
        }
    }

    /// <summary>
    /// Adds the answer options and opening/closing functionality to the selector popups
    /// </summary>
    private void AddAnswerOptions()
    {
        _openMottoOptions.onClick.AddListener(() => { _mottoOptionsPopup.SetActive(true); });

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

        // Get motto options based on character class
        List<string> mottoOptionList = _characterResponseList.GetMottoOptions((CharacterClassID)((_playerData.SelectedCharacterId / 100) * 100));
        foreach (string option in mottoOptionList)
        {
            GameObject optionObject = Instantiate(_answerOptionPrefab, _mottoOptionsPopup.GetComponentInChildren<VerticalLayoutGroup>().transform);
            Button button = optionObject.GetComponent<AnswerOptionHandler>().SetData(option);
            button.onClick.AddListener(() =>
            {
                _MottoText.text = option;
                _mottoOptionsPopup.SetActive(false);
            });
        }

        // Get all defences for choosing the favorite
        IEnumerable<PlayerCharacterPrototype> characters = PlayerCharacterPrototypes.Prototypes.Where(c => c != null);
        foreach (PlayerCharacterPrototype character in characters)
        {
            GameObject defenceOption = Instantiate(_characterOptionPrefab, _characterOptionsPopup.GetComponentInChildren<GridLayoutGroup>().transform);
            Button button = defenceOption.GetComponent<FavoriteDefenceOptionHandler>().SetData(character.Name, character.GalleryImage);
            button.onClick.AddListener(() =>
            {
                _favoriteCharacterImage.sprite = character.GalleryImage;
                _characterOptionsPopup.SetActive(false);
                _closePopupAreaButton.SetActive(false);
            });
        }
    }

    private void Start()
    {
        _saveEditsButton.onClick.AddListener(() =>
        {
            Debug.Log("Save button clicked!");
            SaveInputToFile();
        });

        // Asetetaan URL-painikkeen teksti
        _ClanURLButton.onClick.AddListener(OpenClanURL);

        LoadInputFromFile();
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

            //_BattleCharacter.AddComponent(typeof(Image));
            //Img = Resources.Load<Sprite>(playerData.BattleCharacter.Name);
            //_BattleCharacter.GetComponent<Image>().sprite = Img;
            var store = Storefront.Get();
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p =>
            {
                if (p == null)
                {
                    Debug.LogError("Pelaajatietojen haku epäonnistui.");
                    return;
                }

                _playerData = p;
                _playerNameText.text = _playerData.Name;
                _playerNameInputField.text = _playerData.Name;

                _activityText.text = _playerData.points.ToString();
                //_WinsText.text = _playerData.stats.wonBattles.ToString();

                CharacterClassID defenceClass = (CharacterClassID)((_playerData.SelectedCharacterId / 100) * 100);
                _DefenceClassText.text = defenceClass.ToString();

                store.GetClanData(_playerData.ClanId, clan =>
                {
                    if (clan == null)
                    {
                        _rolesErrorMessage.text = "Klaania ei löydetty.";
                        Debug.LogError("Klaanitietojen haku epäonnistui.");
                        return;
                    }

                    _clanData = clan;
                    _playerClanNameText.text = _clanData.Name;
                    _rolesErrorMessage.text = "Rooleja ei voitu hakea.";

                    if (_ClanButtonText != null)
                    {
                        _clanID = _playerData.ClanId;
                        _url = "https://altzone.fi/clans/" + _playerData.ClanId;
                        _ClanButtonText.text = _playerClanNameText.text; // Asetetaan painikkeen teksti
                    }
                    else
                    {
                        Debug.LogError("Klaanin URL-tekstiobjekti ei ole asetettu.");
                    }
                });
            });

            updateTime();
        }
        else
        {
            Reset();
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



}
