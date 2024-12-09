using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Prefabs.Windows.DefenceScreen;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using System.Linq;
using MenuUi.Scripts.CharacterGallery;
using System;
using Altzone.Scripts.Config;
using System.Threading;


public class CharacterStatWindow : MonoBehaviour
{

    public Sprite[] CharacterArtWork;
    public Image CharacterArtWorkToShow;

    private int UnusedStats;
    private int DiamondSpeedAmount = 100;
    private int DiamondResistanceAmount = 100;
    private int DiamondAttackAmount = 100;
    private int DiamondDefenceAmount = 100;
    private int DiamondHPAmount = 100;
    private int EraserAmount = 100;
    private int SpeedIncreasePrice;
    private int ResistanceIncreasePrice;
    private int AttackIncreasePrice;
    private int DefenceIncreasePrice;
    private int HPIncreasePrice;
    private int CharSizeIncreasePrice = 0;
    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CustomCharacterName;
    [Header("Current stat level?")]
    public TextMeshProUGUI SpeedNumber;
    public TextMeshProUGUI ResistanceNumber;
    public TextMeshProUGUI AttackNumber;
    public TextMeshProUGUI DefenceNumber;
    public TextMeshProUGUI HPNumber;
    public TextMeshProUGUI CharSizeNumber;
    [Header("Amount of diamonds and erasers that can be used")]
    public TextMeshProUGUI DiamondsAmountNumber;
    public TextMeshProUGUI EraserAmountNumber;

    [Header("Not in use anymore?")]
    public TextMeshProUGUI DiamondSpeedAmountNumber;
    public TextMeshProUGUI DiamondResistanceAmountNumber;
    public TextMeshProUGUI DiamondAttackAmountNumber;
    public TextMeshProUGUI DiamondDefenceAmountNumber;
    public TextMeshProUGUI DiamondHPAmountNumber;

    [Header("*********************")]
    [Header("Descriptions")]
    public TextMeshProUGUI CharDescription;//hahmon kuvaus
    public TextMeshProUGUI DefClassSpecial;//defenssiluokan erikoistaidon kuvaus
    private DemoCharacterForStatWindow _demoCharacterWindowCharacter;
    private CustomCharacter _customCharacter;
    private int CurrentlySelectedStat = -1;
    [SerializeField] private TextMeshProUGUI UpgradeCostAmountNumber;
    [SerializeField] private Image UpgradeDiamondImage;

    [Header("Stat editing popup")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private GameObject statEditTab;
    [SerializeField] private Button closeTabButton;
    [SerializeField] private TextMeshProUGUI statIncreasePriceText;
    [SerializeField] private TextMeshProUGUI statDecreasePriceText;

    [Header("Buttons for opening stat editing popup")]
    [SerializeField] private Button impactforce;
    [SerializeField] private Button healthPoints;
    [SerializeField] private Button defence;
    [SerializeField] private Button resistance;
    [SerializeField] private Button charSize;
    [SerializeField] private Button speed;

    [SerializeField] private GalleryCharacterReference _galleryCharacterReference;

    //private BaseCharacter _currentCharacter;
    private PlayerData _playerData;
    private CharacterID _characterId;


    //Mistä löytyy hahmonkuvaus? -Löytyy SetCharacterInfo -metodista
    //Mistä löytyy defenssiluokan kuvaus? -saatavilla, jahka valmistuu

    //Onko olemassa jo tieto käytetyistä tasopykälistä jossain?
    //Ei ole. Palataan myöhemmin.

    //Mitä tarkoittaa stat selected backround?


    private void OnEnable()
    {
        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange += HandleCharacterGalleryCharacterStatWindowToShowChange;



        // Hae CustomCharacter tiedot PlayerDatasta
        _characterId = (CharacterID)SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
        Debug.Log($"Searching for character with ID: {_characterId}");
        //Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, playerData =>  //Alunperin käytti tätä

        //Tällä hetkellä käyttää tätä
        DataStore dataStore = Storefront.Get();
        dataStore.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData =>
        {
            if (playerData == null)
            {

                Debug.Log("GetPlayerData is null");
                return;
            }

            Debug.Log("PlayerData tiedot haettu onnistuneesti");
            _playerData = playerData;

            DiamondSpeedAmount = playerData.DiamondSpeed;
            DiamondResistanceAmount = playerData.DiamondResistance;
            DiamondAttackAmount = playerData.DiamondAttack;
            DiamondDefenceAmount = playerData.DiamondDefence;
            DiamondHPAmount = playerData.DiamondHP;
            EraserAmount = playerData.Eraser;

            Debug.Log("Timanttiarvot asetettu onnistuneesti");

        });
        ActivateStatButtons();
        HandleCharacterGalleryCharacterStatWindowToShowChange(SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow);

    }

    private void OnDisable()
    {
        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange -= HandleCharacterGalleryCharacterStatWindowToShowChange;
        Debug.Log("CharacterStatWindow disabled");
        DisableStatButtons();
    }

    private void HandleCharacterGalleryCharacterStatWindowToShowChange(CharacterID newValue)
    {
        _decideWhatCharacterToShow(newValue);
        SetCharacterStats();

        Debug.Log("handled window change");
    }


    //  upgrade
    private void UpgradeCharacterSpeed()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondSpeedAmount >= SpeedIncreasePrice)
            {
                DiamondSpeedAmount -= SpeedIncreasePrice;
                _playerData.DiamondSpeed = DiamondSpeedAmount;
                DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondSpeedAmount.ToString() + "/" + SpeedIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed += 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);


                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Speed = _demoCharacterWindowCharacter.CharacterSpeed;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }



    private void UpgradeCharacterResistance()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondResistanceAmount >= ResistanceIncreasePrice)
            {
                DiamondResistanceAmount -= ResistanceIncreasePrice;
                _playerData.DiamondResistance = DiamondResistanceAmount;
                DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondResistanceAmount.ToString() + "/" + ResistanceIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterResistance += 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Resistance = _demoCharacterWindowCharacter.CharacterResistance;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterAttack()
    {
        if (CheckMaxLevel() == true)
        {

            if (DiamondAttackAmount >= AttackIncreasePrice)
            {
                DiamondAttackAmount -= AttackIncreasePrice;
                _playerData.DiamondAttack = DiamondAttackAmount;
                DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondAttackAmount.ToString() + "/" + AttackIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterAttack += 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Attack = _demoCharacterWindowCharacter.CharacterAttack;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterDefence()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondDefenceAmount >= DefenceIncreasePrice)
            {
                DiamondDefenceAmount -= DefenceIncreasePrice;
                _playerData.DiamondDefence = DiamondDefenceAmount;
                DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondDefenceAmount.ToString() + "/" + DefenceIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterDefence += 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Defence = _demoCharacterWindowCharacter.CharacterDefence;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void UpgradeCharacterHP()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondHPAmount >= HPIncreasePrice)
            {
                DiamondHPAmount -= HPIncreasePrice;
                _playerData.DiamondHP = DiamondHPAmount;
                DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondHPAmount.ToString() + "/" + HPIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterHP += 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);

                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");
                    customCharacter.Hp = _demoCharacterWindowCharacter.CharacterHP;

                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }

    // degrade
    private void DegradeCharacterSpeed()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterSpeed > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed -= 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Speed = _demoCharacterWindowCharacter.CharacterSpeed;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterResistance()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterResistance > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance -= 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Resistance = _demoCharacterWindowCharacter.CharacterResistance;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }

    private void DegradeCharacterAttack()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterAttack > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack -= 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Attack = _demoCharacterWindowCharacter.CharacterAttack;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterDefence()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterDefence > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence -= 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Defence = _demoCharacterWindowCharacter.CharacterDefence;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }
    private void DegradeCharacterHP()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterHP > 0)
            {
                EraserAmount--;
                _playerData.Eraser = EraserAmount;
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP -= 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();

                var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
                if (customCharacter != null)
                {
                    Debug.Log($"Character found: {customCharacter.Name} with ID: {customCharacter.Id}");

                    customCharacter.Hp = _demoCharacterWindowCharacter.CharacterHP;
                    _playerData.UpdateCustomCharacter(customCharacter);
                }
                else
                {
                    Debug.LogError($"Hahmoa ID:llä {_characterId} ei löytynyt PlayerDatasta.");
                }
            }
        }
    }


    // doing this at awake                                     
    private void _decideWhatCharacterToShow(CharacterID _characterId) //index
    {
        //Method has been changed to use varaible _characterId.
        //Earlier this method used variable "index".

        //var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == index);
        var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
        if (customCharacter == null)
        {
            Debug.LogError($"CustomCharacter not found for index {_characterId}");  //index
            _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
            CharacterArtWorkToShow.sprite = CharacterArtWork[0];
            return;
        }

        //var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)index);
        var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)_characterId);
        if (galleryCharacter == null)
        {

            Debug.LogError($"GalleryCharacterReference not found for index {_characterId}"); //index
            _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
            CharacterArtWorkToShow.sprite = CharacterArtWork[0];
            return;
        }



        switch (_characterId) //index
        {
            case CharacterID.IntellectualizerResearcher:
                SetCharacterInfo();
                break;
            case CharacterID.RetroflectorOvereater:
                SetCharacterInfo();
                break;
            case CharacterID.TricksterComedian:
                SetCharacterInfo();
                break;
            case CharacterID.TricksterConman:
                SetCharacterInfo();
                break;
            case CharacterID.DesensitizerBodybuilder:
                SetCharacterInfo();
                break;
            case CharacterID.ObedientPreacher:
                SetCharacterInfo();
                break;
            case CharacterID.ProjectorGrafitiartist:
                SetCharacterInfo();
                break;
            case CharacterID.ConfluentBesties:
                SetCharacterInfo();
                break;
            case CharacterID.RetroflectorAlcoholic:
                SetCharacterInfo();
                break;
            default:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
                CharacterArtWorkToShow.sprite = CharacterArtWork[0];
                CharDescription.text = "";
                break;
        }
    }

    private void SetCharacterStats()
    {
        if (_demoCharacterWindowCharacter != null)
        {
            CharacterName.text = _demoCharacterWindowCharacter.CharacterName;

            SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
            ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
            AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
            DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
            HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();

            //DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
            DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
            DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
            DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
            DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
            //EraserAmountNumber.text = EraserAmount.ToString();

            UpdatePieChart();
            //UpdateUpgradeButtons();
            //DisableAllStatSelectedBackground();
            Debug.Log("CharacterGallery SetCharacterStats ran");
        }
        else
        {
            Debug.Log("Demo Character Class empty");
        }
    }

    // Checks if levels combined are less than level cap
    private bool CheckMaxLevel()
    {
        if (_demoCharacterWindowCharacter.CharacterSpeed + _demoCharacterWindowCharacter.CharacterResistance + _demoCharacterWindowCharacter.CharacterAttack + _demoCharacterWindowCharacter.CharacterDefence + _demoCharacterWindowCharacter.CharacterHP < 100)
        {
            return true;
        }
        return false;
    }

    // Is used to get an integer for the grey part in pie chart
    public int CheckUnusedStatsAmount()
    {
        UnusedStats = 100 - (_demoCharacterWindowCharacter.CharacterSpeed + _demoCharacterWindowCharacter.CharacterResistance + _demoCharacterWindowCharacter.CharacterAttack + _demoCharacterWindowCharacter.CharacterDefence + _demoCharacterWindowCharacter.CharacterHP);
        return UnusedStats;
    }

    // update pie chart
    private void UpdatePieChart()
    {
        FindObjectOfType<CharacterStatsPieChart>().SetPieChartValues(CharacterStatsIntValuesToFloatValues(_demoCharacterWindowCharacter.CharacterSpeed, _demoCharacterWindowCharacter.CharacterResistance, _demoCharacterWindowCharacter.CharacterAttack, _demoCharacterWindowCharacter.CharacterDefence, _demoCharacterWindowCharacter.CharacterHP, CheckUnusedStatsAmount()));
    }

    private float[] CharacterStatsIntValuesToFloatValues(int characterSpeed, int characterResistance, int characterAttack, int characterDefence, int characterHP, int unusedStats)
    {
        // pie chart works with the floats being in reverse order compared to the image array which holds the pie chart circles, atleast for now 
        float[] characterStatsFloatValues = { characterHP, characterDefence, characterAttack, characterResistance, characterSpeed, unusedStats };
        return characterStatsFloatValues;
    }
    private void SetCharacterInfo()
    {
        var customCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
        var galleryCharacter = _galleryCharacterReference.GetCharacterPrefabInfoFast((int)_characterId);

        //Should this be CustomCharacter?
        _demoCharacterWindowCharacter = new DemoCharacterForStatWindow(galleryCharacter.Name, false,
                   customCharacter.Speed, customCharacter.Resistance, customCharacter.Attack,
                   customCharacter.Defence, customCharacter.Hp);
        CharacterArtWorkToShow.sprite = galleryCharacter.Image;
        Debug.Log($"loaded {galleryCharacter.Name}");

        //Setting stat increasing prices
        AttackIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Attack);
        SpeedIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Speed);
        ResistanceIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Resistance);
        DefenceIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Defence);
        HPIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Hp);
        //CharSizeIncreasePrice = customCharacter.GetPriceToNextLevel()

        //This set the character description and special ability texts.
        //For now these are here.
        switch (_characterId)
        {
            case CharacterID.IntellectualizerResearcher:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.RetroflectorOvereater:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.TricksterComedian:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.TricksterConman:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.DesensitizerBodybuilder:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.ObedientPreacher:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.ProjectorGrafitiartist:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.ConfluentBesties:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.RetroflectorAlcoholic:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            default:
                CharDescription.text = "";
                DefClassSpecial.text = "";
                break;
        }
    }
    private void ActivateStatButtons()
    {

        HideStatEditTab();
        impactforce.onClick.AddListener(() => OnStatButtonClicked(AttackIncreasePrice));
        healthPoints.onClick.AddListener(() => OnStatButtonClicked(HPIncreasePrice));
        defence.onClick.AddListener(() => OnStatButtonClicked(DefenceIncreasePrice));
        resistance.onClick.AddListener(() => OnStatButtonClicked(ResistanceIncreasePrice));
        //charSize.onClick.AddListener(() => OnStatButtonClicked(CharSizeIncreasePrice));
        speed.onClick.AddListener(() => OnStatButtonClicked(SpeedIncreasePrice));
    }
    private void OnStatButtonClicked(int statIncreasePriceToShow)
    {
        increaseButton.onClick.AddListener(() => PlusOrMinusPressed());
        decreaseButton.onClick.AddListener(() => PlusOrMinusPressed());
        closeTabButton.onClick.AddListener(() => HideStatEditTab());
        statIncreasePriceText.text = statIncreasePriceToShow.ToString();

        Debug.Log($"Stat price {statIncreasePriceToShow}");
        statEditTab.SetActive(true);
    }
    private void HideStatEditTab()
    {
        statEditTab.SetActive(false);
    }
    private void PlusOrMinusPressed() //testausta varten
    {
        Debug.Log("painoit plussaa tai miinusta");
    }
    private void DisableStatButtons()
    {
        impactforce.onClick.RemoveAllListeners();
        healthPoints.onClick.RemoveAllListeners();
        defence.onClick.RemoveAllListeners();
        resistance.onClick.RemoveAllListeners();
        charSize.onClick.RemoveAllListeners();
        speed.onClick.RemoveAllListeners();
    }
}
