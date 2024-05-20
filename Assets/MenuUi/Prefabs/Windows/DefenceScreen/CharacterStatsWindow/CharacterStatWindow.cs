using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Prefabs.Windows.DefenceScreen;


public class CharacterStatWindow : MonoBehaviour
{

    public Image CharacterArtWork;

    private int UnusedStats;

    public int DiamondSpeedAmount = 100;
    public int DiamondResistanceAmount = 100;
    public int DiamondAttackAmount = 100;
    public int DiamondDefenceAmount = 100;
    public int DiamondHPAmount = 100;
    public int EraserAmount = 100;

    public int SpeedCostAmount = 5;
    public int ResistanceCostAmount = 5;
    public int AttackCostAmount = 5;
    public int DefenceCostAmount = 5;
    public int HPCostAmount = 5;

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


    DemoCharacterWindowCharacter _demoCharacterWindowCharacter = new DemoCharacterWindowCharacter("Albert Älypää", false, 7, 3, 1, 4, 1);


    private int CurrentlySelectedStat = 0;
    [SerializeField] public Button StatAddButton;
    [SerializeField] public Button StatRemoveButton;
    [SerializeField] public TextMeshProUGUI UpgradeCostAmountNumber;
    [SerializeField] public Image UpgradeDiamondImage;

    // Start is called before the first frame update
    void Start()
    {
        
        SetCharacterStats();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // CurrentlySelectedStat
    public void UpdateCurrentlySelectedStatToSpeed()
    {
        UpgradeButtonsRemoveAllListeners();
        CurrentlySelectedStat = 0;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToResistance()
    {
        UpgradeButtonsRemoveAllListeners();
        CurrentlySelectedStat = 1;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToAttack()
    {
        UpgradeButtonsRemoveAllListeners();
        CurrentlySelectedStat = 2;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToDefence()
    {
        UpgradeButtonsRemoveAllListeners();
        CurrentlySelectedStat = 3;
        UpdateUpgradeButtons();
    }
    public void UpdateCurrentlySelectedStatToHP()
    {
        UpgradeButtonsRemoveAllListeners();
        CurrentlySelectedStat = 4;
        UpdateUpgradeButtons();
    }

    private void UpdateUpgradeButtons()
    {
        switch (CurrentlySelectedStat)
        {
            case 0:
                StatAddButton.onClick.AddListener(UpgradeCharacterSpeed);
                StatRemoveButton.onClick.AddListener(DegradeCharacterSpeed);
                UpgradeDiamondImage.color = new Color32(240,117,117,255);
                UpgradeCostAmountNumber.text = SpeedCostAmount.ToString();
                break;
            case 1:
                StatAddButton.onClick.AddListener(UpgradeCharacterResistance);
                StatRemoveButton.onClick.AddListener(DegradeCharacterResistance);
                UpgradeDiamondImage.color = new Color32(247,178,59,255);
                UpgradeCostAmountNumber.text = ResistanceCostAmount.ToString();
                break;
            case 2:
                StatAddButton.onClick.AddListener(UpgradeCharacterAttack);
                StatRemoveButton.onClick.AddListener(DegradeCharacterAttack);
                UpgradeDiamondImage.color = new Color32(232,56,215,255);
                UpgradeCostAmountNumber.text = AttackCostAmount.ToString();
                break;
            case 3:
                StatAddButton.onClick.AddListener(UpgradeCharacterDefence);
                StatRemoveButton.onClick.AddListener(DegradeCharacterDefence);
                UpgradeDiamondImage.color = new Color32(118,79,234,255);
                UpgradeCostAmountNumber.text = DefenceCostAmount.ToString();
                break;
            case 4:
                StatAddButton.onClick.AddListener(UpgradeCharacterHP);
                StatRemoveButton.onClick.AddListener(DegradeCharacterHP);
                UpgradeDiamondImage.color = new Color32(228,32,35,255);
                UpgradeCostAmountNumber.text = HPCostAmount.ToString();
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
    //  upgrade
    private void UpgradeCharacterSpeed()
    {
        if (CheckMaxLevel() == true)
        {
            if (DiamondSpeedAmount >= SpeedCostAmount)
            {
                DiamondSpeedAmount -= SpeedCostAmount;
                DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed += 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();
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
                DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance += 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();
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
                DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack += 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();
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
                DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence += 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();
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
                DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP += 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();
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


    // set at start
    private void SetCharacterStats()
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
    }

    // Check max level
    private bool CheckMaxLevel()
    {
        if (_demoCharacterWindowCharacter.CharacterSpeed + _demoCharacterWindowCharacter.CharacterResistance + _demoCharacterWindowCharacter.CharacterAttack + _demoCharacterWindowCharacter.CharacterDefence + _demoCharacterWindowCharacter.CharacterHP < 100)
        {
            return true;
        } 
        return false;
    }

    // Check Unused stats amount
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
        // stat pie chart works with the stats being in reverse order, atleast for now 
        float[] characterStatsFloatValues = { characterHP, characterDefence, characterAttack, characterResistance, characterSpeed, unusedStats };
        return characterStatsFloatValues;
    }

    // Character Name
    public void CharacterNameChange()
    {
        _demoCharacterWindowCharacter.CharacterName = CharacterName.text;
    }
}
