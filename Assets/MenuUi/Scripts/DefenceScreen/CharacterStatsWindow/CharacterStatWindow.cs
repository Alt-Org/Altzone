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
    public Image CharacterArtWorkForInfoCanva;

    private int UnusedStats;
    private int DiamondSpeedAmount = 100;
    private int DiamondResistanceAmount = 100;
    private int DiamondAttackAmount = 100;
    private int DiamondDefenceAmount = 100;
    private int DiamondHPAmount = 100;
    private int EraserAmount = 100;
    private int diamondsAmount = 1000;
    private int SpeedIncreasePrice;
    private int ResistanceIncreasePrice;
    private int AttackIncreasePrice;
    private int DefenceIncreasePrice;
    private int HPIncreasePrice;
    private int CharSizeIncreasePrice;
    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CustomCharacterName;
    [Header("Current stat level for statpage")]
    public TextMeshProUGUI SpeedNumber;
    public TextMeshProUGUI ResistanceNumber;
    public TextMeshProUGUI AttackNumber;
    public TextMeshProUGUI DefenceNumber;
    public TextMeshProUGUI HPNumber;
    public TextMeshProUGUI CharSizeNumber;

    [Header("Amount of diamonds and erasers that can be used")]
    public TextMeshProUGUI DiamondsAmountNumberText;
    public TextMeshProUGUI EraserAmountNumber;

    [Header("Stat info for infopage")]
    public TextMeshProUGUI impactforceCurrentLevel;
    public TextMeshProUGUI healthPointsCurrentLevel;
    public TextMeshProUGUI resistanceCurrentLevel;
    public TextMeshProUGUI defenceCurrentLevel;
    public TextMeshProUGUI charSizeCurrentLevel;
    public TextMeshProUGUI speedCurrentLevel;

    [Header("Descriptions")]
    public TextMeshProUGUI CharDescription;//character description
    public TextMeshProUGUI DefClassSpecial;//defence class description
    private DemoCharacterForStatWindow _demoCharacterWindowCharacter;
    private int CurrentlySelectedStat = -1;
    [SerializeField] private TextMeshProUGUI UpgradeCostAmountNumber;
    [SerializeField] private Image UpgradeDiamondImage;

    [Header("Stat editing popup")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private GameObject statEditPopUp;
    [SerializeField] private Button closePopUpButton;

    [SerializeField] private TextMeshProUGUI statIncreasePriceText;

    [Header("Buttons for opening stat editing popup")]
    [SerializeField] private Button impactforce;
    [SerializeField] private Button healthPoints;
    [SerializeField] private Button defence;
    [SerializeField] private Button resistance;
    [SerializeField] private Button charSize;
    [SerializeField] private Button speed;

    [SerializeField] private GalleryCharacterReference _galleryCharacterReference;

    private PlayerData _playerData;
    private CharacterID _characterId;


    //Working methods for increasing and decreasing stat level
    //Diamonds for each stat not in use anymore. Now there's only one amount of diamonds that is used
    //to buy new statpoints. Not implemented yet. Old codes are commented out.
    //Serializefields must be check over to see if there's something not needed anymore.
    //Amounts for victories and losses needs to be implemented.
    //character size stat needs to be implemented.



    private void OnEnable()
    {
        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange += HandleCharacterGalleryCharacterStatWindowToShowChange;
        ActivateStatButtons();
        _characterId = (CharacterID)SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
        Debug.Log($"Searching for character with ID: {_characterId}");

        //Used this before
        //Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, playerData =>  //Alunperin käytti tätä
        //Uses this now
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
            if (diamondsAmount >= SpeedIncreasePrice)
            {

                HandleDiamondAmountChange(SpeedIncreasePrice);

                UpgradeCostAmountNumber.text = DiamondSpeedAmount.ToString() + "/" + SpeedIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed += 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                speedCurrentLevel.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();

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
            if (diamondsAmount >= ResistanceIncreasePrice)
            {

                HandleDiamondAmountChange(ResistanceIncreasePrice);

                UpgradeCostAmountNumber.text = DiamondResistanceAmount.ToString() + "/" + ResistanceIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterResistance += 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                resistanceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();

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

            if (diamondsAmount >= AttackIncreasePrice)
            {

                HandleDiamondAmountChange(AttackIncreasePrice);

                UpgradeCostAmountNumber.text = DiamondAttackAmount.ToString() + "/" + AttackIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterAttack += 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                impactforceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();

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
            if (diamondsAmount >= DefenceIncreasePrice)
            {

                HandleDiamondAmountChange(DefenceIncreasePrice);

                UpgradeCostAmountNumber.text = DiamondDefenceAmount.ToString() + "/" + DefenceIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterDefence += 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                defenceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();

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
            if (diamondsAmount >= HPIncreasePrice)
            {

                HandleDiamondAmountChange(HPIncreasePrice);

                UpgradeCostAmountNumber.text = DiamondHPAmount.ToString() + "/" + HPIncreasePrice.ToString();
                _demoCharacterWindowCharacter.CharacterHP += 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                healthPointsCurrentLevel.text = _demoCharacterWindowCharacter.CharacterHP.ToString();

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
                impactforceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();

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
                resistanceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();

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
                impactforceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();

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
                defenceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();

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
                healthPointsCurrentLevel.text = _demoCharacterWindowCharacter.CharacterHP.ToString();

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
            case CharacterID.Booksmart:
                SetCharacterInfo();
                break;
            case CharacterID.Overeater:
                SetCharacterInfo();
                break;
            case CharacterID.Joker:
                SetCharacterInfo();
                break;
            case CharacterID.Conman:
                SetCharacterInfo();
                break;
            case CharacterID.Bodybuilder:
                SetCharacterInfo();
                break;
            case CharacterID.Religious:
                SetCharacterInfo();
                break;
            case CharacterID.Artist:
                SetCharacterInfo();
                break;
            case CharacterID.Soulsisters:
                SetCharacterInfo();
                break;
            case CharacterID.Alcoholic:
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
            CustomCharacterName.text = CharacterName.text;

            SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
            ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
            AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
            DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
            HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();




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
        CharacterArtWorkForInfoCanva.sprite = galleryCharacter.Image;


        Debug.Log($"loaded {galleryCharacter.Name}");

        //For the right side window. CharSize not impelemented yet.
        impactforceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
        resistanceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
        speedCurrentLevel.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
        defenceCurrentLevel.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
        healthPointsCurrentLevel.text = _demoCharacterWindowCharacter.CharacterHP.ToString();

        //Getting stat increasing prices
        AttackIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Attack);
        SpeedIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Speed);
        ResistanceIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Resistance);
        DefenceIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Defence);
        HPIncreasePrice = customCharacter.GetPriceToNextLevel(StatType.Hp);
        //CharSizeIncreasePrice = customCharacter.GetPriceToNextLevel();

        //This set the character description and special ability texts.
        switch (_characterId)
        {
            case CharacterID.Booksmart:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Overeater:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Joker:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Conman:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Bodybuilder:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Religious:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Artist:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Soulsisters:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            case CharacterID.Alcoholic:
                CharDescription.text = "Hahmon kuvausteksti tulee tähän, kun tiedetään mitä tähän pitää kirjoittaa.";
                DefClassSpecial.text = "Erikoistaidon kuvausteksti tulee tähän, sitten aikanaan.";
                break;
            default:
                CharDescription.text = "";
                DefClassSpecial.text = "";
                break;
        }
    }

    //Button finctionality for stat editing popup
    private void ActivateStatButtons()
    {
        HidestatEditPopUp();
        impactforce.onClick.AddListener(() => EditStatImpactforce());
        healthPoints.onClick.AddListener(() => EditStatHealthPoints());
        defence.onClick.AddListener(() => EditStatDefence());
        resistance.onClick.AddListener(() => EditStatResistance());
        //charSize.onClick.AddListener(() => EditStatCharSize();
        speed.onClick.AddListener(() => EditStatSpeed());
    }
    private void EditStatImpactforce()
    {
        HidestatEditPopUp();
        ResetPlusAndMinus();
        SetCorrectStatPrice(AttackIncreasePrice);
        closePopUpButton.onClick.AddListener(HidestatEditPopUp);
        increaseButton.onClick.AddListener(UpgradeCharacterAttack);
        decreaseButton.onClick.AddListener(DegradeCharacterAttack);
    }
    private void EditStatHealthPoints()
    {
        HidestatEditPopUp();
        ResetPlusAndMinus();
        SetCorrectStatPrice(HPIncreasePrice);
        closePopUpButton.onClick.AddListener(HidestatEditPopUp);
        increaseButton.onClick.AddListener(UpgradeCharacterHP);
        decreaseButton.onClick.AddListener(DegradeCharacterHP);
    }
    private void EditStatResistance()
    {
        HidestatEditPopUp();
        ResetPlusAndMinus();
        SetCorrectStatPrice(ResistanceIncreasePrice);
        closePopUpButton.onClick.AddListener(HidestatEditPopUp);
        increaseButton.onClick.AddListener(UpgradeCharacterResistance);
        decreaseButton.onClick.AddListener(DegradeCharacterResistance);
    }
    /*  private void EditStatCharSize()
     {
         HidestatEditPopUp();
         ResetPlusAndMinus();
         SetCorrectStatPrice(CharSizeIncreasePrice);
         increaseButton.onClick.AddListener();
     } */
    private void EditStatDefence()
    {
        HidestatEditPopUp();
        ResetPlusAndMinus();
        SetCorrectStatPrice(DefenceIncreasePrice);
        closePopUpButton.onClick.AddListener(HidestatEditPopUp);
        increaseButton.onClick.AddListener(UpgradeCharacterDefence);
        decreaseButton.onClick.AddListener(DegradeCharacterDefence);
    }
    private void EditStatSpeed()
    {
        ResetPlusAndMinus();
        HidestatEditPopUp();
        SetCorrectStatPrice(SpeedIncreasePrice);
        closePopUpButton.onClick.AddListener(HidestatEditPopUp);
        increaseButton.onClick.AddListener(UpgradeCharacterSpeed);
        decreaseButton.onClick.AddListener(DegradeCharacterSpeed);
    }
    private void SetCorrectStatPrice(int statIncreasePriceToShow)
    {
        statIncreasePriceText.text = $"{statIncreasePriceToShow}";
        Debug.Log($"Stat price {statIncreasePriceToShow}");

        if (CustomCharacter.GetClassID(_characterId) != CharacterClassID.Obedient)
        {
            statEditPopUp.SetActive(true);
        }
    }
    private void HidestatEditPopUp()
    {
        ResetPlusAndMinus();
        statEditPopUp.SetActive(false);
    }
    private void ResetPlusAndMinus()
    {
        increaseButton.onClick.RemoveAllListeners();
        decreaseButton.onClick.RemoveAllListeners();
    }
    private void DisableStatButtons()
    {
        ResetPlusAndMinus();
        impactforce.onClick.RemoveAllListeners();
        healthPoints.onClick.RemoveAllListeners();
        defence.onClick.RemoveAllListeners();
        resistance.onClick.RemoveAllListeners();
        charSize.onClick.RemoveAllListeners();
        speed.onClick.RemoveAllListeners();
    }
    private void HandleDiamondAmountChange(int statIncreasePrice)
    {
        if (diamondsAmount >= statIncreasePrice)
        {
            diamondsAmount -= statIncreasePrice;
        }
        else
        {
            Debug.Log("Not enough diamonds");
        }
        DiamondsAmountNumberText.text = diamondsAmount.ToString();

    }
}
