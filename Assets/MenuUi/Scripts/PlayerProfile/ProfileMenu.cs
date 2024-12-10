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
    [SerializeField] private TextMeshProUGUI _playerClanNameText;
    [SerializeField] private TextMeshProUGUI _MottoText;
    [SerializeField] private TextMeshProUGUI _TimePlayedText;
    [SerializeField] private TextMeshProUGUI _LosesText;
    [SerializeField] private TextMeshProUGUI _WinsText;
    [SerializeField] private TextMeshProUGUI _CarbonText;
    [SerializeField] private TMP_InputField _LifeQuoteInputField;
    [SerializeField] private TMP_InputField _LoreInputField;



    [Header("savebutton")]
    [SerializeField] private Button _saveEditsButton;


    [SerializeField] private PlayStyle _playStyle;

    //[Header("Images")]
    // [SerializeField] private GameObject _BattleCharacter = new GameObject();

    // private Sprite Img;

    private int tempLocalSaveTime;
    private float tempLocalSaveSecondsTime;
    private float tempLocalSaveToCarbon;
    private float tempLocalSaveCarbon;


    private int minuteCount;
    private float secondsCount;
    private float countToCarbon;
    private float carbonCount;


    private float kgCarbon => CarbonFootprint.CarbonCount / 1000f;

    private PlayerData _playerData = null;
    private ClanData _clanData = null;


    private const string _DarkOrange = "#FF6A00";
    private const string _Orange = "#FFA100";




    private ServerPlayer _player;

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



        // Tarkistaa onko kg vai g
        float carbonDisplay = CarbonFootprint.CarbonCount;
        string carbonUnit = "g";

        if (carbonDisplay >= 1000f)
        {
            carbonDisplay /= 1000f;
            carbonUnit = "kg";
        }

        _CarbonText.text = $"Hiilijalanjälki\n{carbonDisplay:F1}{carbonUnit}/CO2"; // Hiilijalanjälki teksti

        // Päivittää minuutin välein peliajan.
        if (secondsCount >= 60f)
        {
            minuteCount++;
            secondsCount = 0;
        }
    }

    private void OnEnable()
    {
        Debug.Log($"Initial LifeQuote text: {_LifeQuoteInputField.text}");
        LoadInputFromFile();

        //tempLocalSaveTime
        ServerManager.OnLogInStatusChanged += SetPlayerProfileValues;
        _player = ServerManager.Instance.Player;

        if (_player == null)
            SetPlayerProfileValues(false);
        else
            SetPlayerProfileValues(true);



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




    //  Saves lifequote and lore inputfields. (Playstyle saving is in its own file, doesn't require clicking the save button
    public void SaveInputToFile()
    {
        if (_LifeQuoteInputField != null)
        {
            string lifeQuote = _LifeQuoteInputField.text;
            string path = Path.Combine(Application.persistentDataPath, "LifeQuote.txt");
            File.WriteAllText(path, lifeQuote);
            Debug.Log($"Saved Life Quote: {lifeQuote} at {path}");
        }

        if (_LoreInputField != null)
        {
            string lore = _LoreInputField.text;
            string path = Path.Combine(Application.persistentDataPath, "Lore.txt");
            File.WriteAllText(path, lore);
            Debug.Log($"Saved Lore: {lore} at {path}");
        }
    }

    // Loads saved files.
    public void LoadInputFromFile()
    {
        string quotePath = Path.Combine(Application.persistentDataPath, "LifeQuote.txt");
        string lorePath = Path.Combine(Application.persistentDataPath, "Lore.txt");
        if (File.Exists(quotePath))
        {
            string loadedQuote = File.ReadAllText(quotePath);
            Debug.Log($"Loaded Life Quote: {loadedQuote} from {quotePath}");
            if (_LifeQuoteInputField != null)
            {
                _LifeQuoteInputField.text = loadedQuote;
            }
        }
        else
        {
            Debug.Log("No LifeQuote file found.");
        }

        if (File.Exists(lorePath))
        {
            string loadedLore = File.ReadAllText(lorePath);
            Debug.Log($"Loaded Life Quote: {loadedLore} from {lorePath}");
            if (_LoreInputField != null)
            {
                _LoreInputField.text = loadedLore;
            }
        }
    }




    private void Start()
    {
        _saveEditsButton.onClick.AddListener(() =>
        {
            Debug.Log("Save button clicked!");
            SaveInputToFile();
        });

        LoadInputFromFile();
    }





    /// <summary>
    /// Sets the name of the Player and stats when log in status changes
    /// </summary>
    /// <param name="isLoggedIn">Logged in status</param>
    private void SetPlayerProfileValues(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // Gets the player name from DataStorage
            var store = Storefront.Get();
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerData = p);
            store.GetClanData(_playerData.ClanId, p => _clanData = p);
            _playerNameText.text = _playerData.Name;
            _playerClanNameText.text = _clanData.Name;

            //_BattleCharacter.AddComponent(typeof(Image));
            //Img = Resources.Load<Sprite>(playerData.BattleCharacter.Name);
            //_BattleCharacter.GetComponent<Image>().sprite = Img;
            updateTime();
            //_LosesWinsText.text = ;
        }
        else
        {
            Reset();
        }
    }


}

