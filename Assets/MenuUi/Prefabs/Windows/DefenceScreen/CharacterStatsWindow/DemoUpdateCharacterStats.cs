using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Prefabs.Windows.DefenceScreen;

public class DemoUpdateCharacterStats : MonoBehaviour
{

    public Image CharacterArtWork;
    public int UnusedStats;

    public int DiamondSpeedAmount = 100;
    public int DiamondResistanceAmount = 100;
    public int DiamondAttackAmount = 100;
    public int DiamondDefenceAmount = 100;
    public int DiamondHPAmount = 100;
    public int EraserAmount = 100;

    public int SpeedCostAmount = 11;
    public int ResistanceCostAmount = 9;
    public int AttackCostAmount = 7;
    public int DefenceCostAmount = 5;
    public int HPCostAmount = 5;

    public TextMeshProUGUI CharacterName;

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

    public TextMeshProUGUI SpeedCostAmountNumber;
    public TextMeshProUGUI ResistanceCostAmountNumber;
    public TextMeshProUGUI AttackCostAmountNumber;
    public TextMeshProUGUI DefenceCostAmountNumber;
    public TextMeshProUGUI HPCostAmountNumber;

    public int CharacterGalleryInt;
    DemoCharacterWindowCharacter _demoCharacterWindowCharacter = new DemoCharacterWindowCharacter("Albert Älypää", false, 7, 3, 1, 4, 1);


    // Start is called before the first frame update
    void Start()
    {
        
        SetCharacterStats();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //  upgrade 
    public void UpgradeCharacterSpeed()
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

    public void UpgradeCharacterResistance()
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
    public void UpgradeCharacterAttack()
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
    public void UpgradeCharacterDefence()
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
    public void UpgradeCharacterHP()
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
    public void DegradeCharacterSpeed()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterSpeed > 0)
            {
                EraserAmount--;
                EraserAmountNumber.text = EraserAmount.ToString();
                DiamondSpeedAmount += SpeedCostAmount;
                DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
                _demoCharacterWindowCharacter.CharacterSpeed -= 1;
                SpeedNumber.text = _demoCharacterWindowCharacter.CharacterSpeed.ToString();
                UpdatePieChart();
            }
        }
    }
    public void DegradeCharacterResistance()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterResistance > 0)
            {
                EraserAmount--;
                EraserAmountNumber.text = EraserAmount.ToString();
                DiamondResistanceAmount += ResistanceCostAmount;
                DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
                _demoCharacterWindowCharacter.CharacterResistance -= 1;
                ResistanceNumber.text = _demoCharacterWindowCharacter.CharacterResistance.ToString();
                UpdatePieChart();
            }
        }
    }
    public void DegradeCharacterAttack()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterAttack > 0)
            {
                EraserAmount--;
                EraserAmountNumber.text = EraserAmount.ToString();
                DiamondAttackAmount += AttackCostAmount;
                DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
                _demoCharacterWindowCharacter.CharacterAttack -= 1;
                AttackNumber.text = _demoCharacterWindowCharacter.CharacterAttack.ToString();
                UpdatePieChart();
            }
        }
    }
    public void DegradeCharacterDefence()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterDefence > 0)
            {
                EraserAmount--;
                EraserAmountNumber.text = EraserAmount.ToString();
                DiamondDefenceAmount += DefenceCostAmount;
                DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
                _demoCharacterWindowCharacter.CharacterDefence -= 1;
                DefenceNumber.text = _demoCharacterWindowCharacter.CharacterDefence.ToString();
                UpdatePieChart();
            }
        }
    }
    public void DegradeCharacterHP()
    {
        if (EraserAmount >= 1)
        {
            if (_demoCharacterWindowCharacter.CharacterHP > 0)
            {
                EraserAmount--;
                EraserAmountNumber.text = EraserAmount.ToString();
                DiamondHPAmount += HPCostAmount;
                DiamondHPAmountNumber.text = DiamondHPAmount.ToString();
                _demoCharacterWindowCharacter.CharacterHP -= 1;
                HPNumber.text = _demoCharacterWindowCharacter.CharacterHP.ToString();
                UpdatePieChart();
            }
        }
    }


    // set at start
    public void SetCharacterStats()
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

        SpeedCostAmountNumber.text = SpeedCostAmount.ToString();
        ResistanceCostAmountNumber.text = ResistanceCostAmount.ToString();
        AttackCostAmountNumber.text = AttackCostAmount.ToString();
        DefenceCostAmountNumber.text = DefenceCostAmount.ToString();
        HPCostAmountNumber.text = HPCostAmount.ToString();

        UpdatePieChart();
    }

    // Check max level
    public bool CheckMaxLevel()
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
    public void UpdatePieChart()
    {
        FindObjectOfType<CharacterStatsPieChart>().SetPieChartValues(CharacterStatsIntValuesToFloatValues(_demoCharacterWindowCharacter.CharacterSpeed, _demoCharacterWindowCharacter.CharacterResistance, _demoCharacterWindowCharacter.CharacterAttack, _demoCharacterWindowCharacter.CharacterDefence, _demoCharacterWindowCharacter.CharacterHP, CheckUnusedStatsAmount()));
    }

    public float[] CharacterStatsIntValuesToFloatValues(int characterSpeed, int characterResistance, int characterAttack, int characterDefence, int characterHP, int unusedStats)
    {
        // stat pie chart works with the stats being in reverse order, atleast for now 
        float[] characterStatsFloatValues = { characterHP, characterDefence, characterAttack, characterResistance, characterSpeed, unusedStats };
        return characterStatsFloatValues;
    }

}
