using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Prefabs.Windows.DefenceScreen;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using System.Linq;


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

    private int SpeedCostAmount = 5;
    private int ResistanceCostAmount = 5;
    private int AttackCostAmount = 5;
    private int DefenceCostAmount = 5;
    private int HPCostAmount = 5;

    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CustomCharacterName;

    public TextMeshProUGUI SpeedNumber;
    public TextMeshProUGUI ResistanceNumber;
    public TextMeshProUGUI AttackNumber;
    public TextMeshProUGUI DefenceNumber;
    public TextMeshProUGUI HPNumber;

    public TextMeshProUGUI DiamondSpeedAmountNumber;
    public TextMeshProUGUI DiamondResistanceAmountNumber;
    public TextMeshProUGUI DiamondAttackAmountNumber;
    public TextMeshProUGUI DiamondDefenceAmountNumber;
    public TextMeshProUGUI DiamondHPAmountNumber;
    public TextMeshProUGUI EraserAmountNumber;

    private DemoCharacterForStatWindow _demoCharacterWindowCharacter;

    private int CurrentlySelectedStat = -1;
    [SerializeField] public Button StatAddButton;
    [SerializeField] public Button StatRemoveButton;
    [SerializeField] public TextMeshProUGUI UpgradeCostAmountNumber;
    [SerializeField] private Image UpgradeDiamondImage;

    [SerializeField] private Image _statSpeedSelectedBackground;
    [SerializeField] private Image _statResistanceSelectedBackground;
    [SerializeField] private Image _statAttackSelectedBackground;
    [SerializeField] private Image _statDefenceSelectedBackground;
    [SerializeField] private Image _statHPSelectedBackground;

    private PlayerData _playerData;
    private CharacterID _characterId;

    private void OnEnable()
    {
        SettingsCarrier.Instance.OnCharacterGalleryCharacterStatWindowToShowChange += HandleCharacterGalleryCharacterStatWindowToShowChange;
        Debug.Log("CharacterStatWindow enabled");

        // Hae CustomCharacter tiedot PlayerDatasta
        _characterId = (CharacterID)SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
        Debug.Log($"Searching for character with ID: {_characterId}");

        Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, playerData =>
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
    }

    private void HandleCharacterGalleryCharacterStatWindowToShowChange(CharacterID newValue)
    {
        _decideWhatCharacterToShow(newValue);
        SetCharacterStats();
        Debug.Log("handled window change");
    }

    // CurrentlySelectedStat
    public void UpdateCurrentlySelectedStatToSpeed()
    {
        UpgradeButtonsRemoveAllListeners();
        DisableAllStatSelectedBackground();
        CurrentlySelectedStat = 0;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToResistance()
    {
        UpgradeButtonsRemoveAllListeners();
        DisableAllStatSelectedBackground();
        CurrentlySelectedStat = 1;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToAttack()
    {
        UpgradeButtonsRemoveAllListeners();
        DisableAllStatSelectedBackground();
        CurrentlySelectedStat = 2;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToDefence()
    {
        UpgradeButtonsRemoveAllListeners();
        DisableAllStatSelectedBackground();
        CurrentlySelectedStat = 3;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToHP()
    {
        UpgradeButtonsRemoveAllListeners();
        DisableAllStatSelectedBackground();
        CurrentlySelectedStat = 4;
        UpdateUpgradeButtons();
    }

    private void UpdateUpgradeButtons()
    {
        switch (CurrentlySelectedStat)
        {
            case -1:
                UpgradeDiamondImage.color = new Color32(100, 100, 100, 255);
                UpgradeCostAmountNumber.text = "";
                break;
            case 0:
                StatAddButton.onClick.AddListener(UpgradeCharacterSpeed);
                StatRemoveButton.onClick.AddListener(DegradeCharacterSpeed);
                UpgradeDiamondImage.color = new Color32(240,117,117,255);
                UpgradeCostAmountNumber.text = DiamondSpeedAmount.ToString() + "/" + SpeedCostAmount.ToString();
                _statSpeedSelectedBackground.enabled = true;
                break;
            case 1:
                StatAddButton.onClick.AddListener(UpgradeCharacterResistance);
                StatRemoveButton.onClick.AddListener(DegradeCharacterResistance);
                UpgradeDiamondImage.color = new Color32(247,178,59,255);
                UpgradeCostAmountNumber.text = DiamondResistanceAmount.ToString() + "/" + ResistanceCostAmount.ToString();
                _statResistanceSelectedBackground.enabled = true;
                break;
            case 2:
                StatAddButton.onClick.AddListener(UpgradeCharacterAttack);
                StatRemoveButton.onClick.AddListener(DegradeCharacterAttack);
                UpgradeDiamondImage.color = new Color32(232,56,215,255);
                UpgradeCostAmountNumber.text = DiamondAttackAmount.ToString() + "/" + AttackCostAmount.ToString();
                _statAttackSelectedBackground.enabled = true;
                break;
            case 3:
                StatAddButton.onClick.AddListener(UpgradeCharacterDefence);
                StatRemoveButton.onClick.AddListener(DegradeCharacterDefence);
                UpgradeDiamondImage.color = new Color32(118,79,234,255);
                UpgradeCostAmountNumber.text = DiamondDefenceAmount.ToString() + "/" + DefenceCostAmount.ToString();
                _statDefenceSelectedBackground.enabled = true;
                break;
            case 4:
                StatAddButton.onClick.AddListener(UpgradeCharacterHP);
                StatRemoveButton.onClick.AddListener(DegradeCharacterHP);
                UpgradeDiamondImage.color = new Color32(228,32,35,255);
                UpgradeCostAmountNumber.text = DiamondHPAmount.ToString() + "/" + HPCostAmount.ToString();
                _statHPSelectedBackground.enabled = true;
                break;
            default:
                Debug.Log("CurrentlySelecterStat is probably somewhere it shouldn't be :/");
                break;
        }
    }

    private void UpgradeButtonsRemoveAllListeners()
    {
        StatAddButton.onClick.RemoveAllListeners();
        StatRemoveButton.onClick.RemoveAllListeners();
    }
    private void DisableAllStatSelectedBackground()
    {
        _statSpeedSelectedBackground.enabled = false;
        _statResistanceSelectedBackground.enabled = false;
        _statAttackSelectedBackground.enabled = false;
        _statDefenceSelectedBackground.enabled = false;
        _statHPSelectedBackground.enabled = false;
    }
    //  upgrade
    private void UpgradeCharacterSpeed()
    {
        if (CheckMaxLevel() == true)            
        {
            if (DiamondSpeedAmount >= SpeedCostAmount)
            {
                DiamondSpeedAmount -= SpeedCostAmount;
                _playerData.DiamondSpeed = DiamondSpeedAmount;
                DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondSpeedAmount.ToString() + "/" + SpeedCostAmount.ToString();
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
            if (DiamondResistanceAmount >= ResistanceCostAmount)
            {
                DiamondResistanceAmount -= ResistanceCostAmount;
                _playerData.DiamondResistance = DiamondResistanceAmount;
                DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondResistanceAmount.ToString() + "/" + ResistanceCostAmount.ToString();
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

            if (DiamondAttackAmount >= AttackCostAmount)
            {
                DiamondAttackAmount -= AttackCostAmount;
                _playerData.DiamondAttack = DiamondAttackAmount;
                DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondAttackAmount.ToString() + "/" + AttackCostAmount.ToString();
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
            if (DiamondDefenceAmount >= DefenceCostAmount)
            {
                DiamondDefenceAmount -= DefenceCostAmount;
                _playerData.DiamondDefence = DiamondDefenceAmount;
                DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondDefenceAmount.ToString() + "/" + DefenceCostAmount.ToString();
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
            if (DiamondHPAmount >= HPCostAmount)
            {
                DiamondHPAmount -= HPCostAmount;
                _playerData.DiamondHP = DiamondHPAmount;
                DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
                UpgradeCostAmountNumber.text = DiamondHPAmount.ToString() + "/" + HPCostAmount.ToString();
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
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed -= 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();
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
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance -= 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();
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
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack -= 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();
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
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence -= 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();
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
                EraserAmountNumber.text = EraserAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP -= 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();
            }
        }
    }


    // doing this at awake
    private void _decideWhatCharacterToShow(CharacterID index)
    {
        switch (index)
        {
            case CharacterID.IntellectualizerResearcher:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Albert Älypää", false, 7, 3, 1, 4, 1);
                CharacterArtWorkToShow.sprite = CharacterArtWork[0];
                Debug.Log("loaded Albert");
                break;
            case CharacterID.RetroflectorOvereater:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Hannu-Hodari", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[1];
                Debug.Log("loaded Hannu");
                break;
            case CharacterID.TricksterComedian:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Vitsi-Ville", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[2];
                Debug.Log("loaded Ville");
                break;
            case CharacterID.TricksterConman:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Lasse Liukas", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[8];
                Debug.Log("loaded Lasse");
                break;
            case CharacterID.DesensitizerBodybuilder:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Rauta-Rami", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[3];
                Debug.Log("loaded Rami");
                break;
            case CharacterID.ObedientPreacher:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Sami Saarnaaja", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[4];
                Debug.Log("loaded Sami");
                break;
            case CharacterID.ProjectorGrafitiartist:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Graffit-Gaya", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[5];
                Debug.Log("loaded Gaya");
                break;
            case CharacterID.ConfluentBesties:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Tiimitytöt", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[6];
                Debug.Log("loaded Tytöt");
                break;
            case CharacterID.RetroflectorAlcoholic:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("Pullo-Piraatti", false, 2, 2, 2, 2, 2);
                CharacterArtWorkToShow.sprite = CharacterArtWork[7];
                Debug.Log("loaded Piraatti");
                break;
            default:
                _demoCharacterWindowCharacter = new DemoCharacterForStatWindow("NotACharacter", false, 10, 10, 10, 10, 10);
                CharacterArtWorkToShow.sprite = CharacterArtWork[0];
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

            DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
            DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
            DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
            DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
            DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
            EraserAmountNumber.text = EraserAmount.ToString();

            UpdatePieChart();
            UpdateUpgradeButtons();
            DisableAllStatSelectedBackground();
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


    // Character Name. DISABLED FOR THE SAKE OF TESTERS
    /*
    public void CharacterNameChange()
    {
        _demoCharacterWindowCharacter.CharacterName = CharacterName.text;
    }
    */
}
