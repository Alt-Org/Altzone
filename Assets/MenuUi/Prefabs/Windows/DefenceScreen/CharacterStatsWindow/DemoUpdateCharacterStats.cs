using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;

public class DemoUpdateCharacterStats : MonoBehaviour
{
    public Image CharacterArtWork;
    public bool Favourite = false;

    public int DiamondSpeedAmount = 100;
    public int DiamondResistanceAmount = 100;
    public int DiamondAttackAmount = 100;
    public int DiamondDefenceAmount = 100;

    public int SpeedCostAmount = 11;
    public int ResistanceCostAmount = 9;
    public int AttackCostAmount = 7;
    public int DefenceCostAmount = 5;
    
    public TextMeshProUGUI CharacterName;

    public TextMeshProUGUI SpeedNumber;
    public TextMeshProUGUI ResistanceNumber;
    public TextMeshProUGUI AttackNumber;
    public TextMeshProUGUI DefenceNumber;

    public TextMeshProUGUI DiamondSpeedAmountNumber;
    public TextMeshProUGUI DiamondResistanceAmountNumber;
    public TextMeshProUGUI DiamondAttackAmountNumber;
    public TextMeshProUGUI DiamondDefenceAmountNumber;

    public TextMeshProUGUI SpeedCostAmountNumber;
    public TextMeshProUGUI ResistanceCostAmountNumber;
    public TextMeshProUGUI AttackCostAmountNumber;
    public TextMeshProUGUI DefenceCostAmountNumber;


    CustomCharacter _customCharacter = new CustomCharacter("id", "classId", "unityKey", "namenamename", 3, 2, 5, 7);


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
        if (DiamondSpeedAmount >= SpeedCostAmount)
        {
            DiamondSpeedAmount -= SpeedCostAmount;
            DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
            _customCharacter.Speed += 1;
            SpeedNumber.text = _customCharacter.Speed.ToString();
        }
    }

    public void UpgradeCharacterResistance()
    {
        if (DiamondResistanceAmount >= ResistanceCostAmount)
        {
            DiamondResistanceAmount -= ResistanceCostAmount;
            DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
            _customCharacter.Resistance += 1;
            ResistanceNumber.text = _customCharacter.Resistance.ToString();
        }
    }
    public void UpgradeCharacterAttack()
    {
        if (DiamondAttackAmount >= AttackCostAmount)
        {
            DiamondAttackAmount -= AttackCostAmount;
            DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
            _customCharacter.Attack += 1;
            AttackNumber.text = _customCharacter.Attack.ToString();
        }
    }
    public void UpgradeCharacterDefence()
    {
        if (DiamondDefenceAmount >= DefenceCostAmount)
        {
            DiamondDefenceAmount -= DefenceCostAmount;
            DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
            _customCharacter.Defence += 1;
            DefenceNumber.text = _customCharacter.Defence.ToString();
        }
    }

    // degrade
    public void DegradeCharacterSpeed()
    {
        if (_customCharacter.Speed > 0)
        {
            DiamondSpeedAmount += SpeedCostAmount;
            DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
            _customCharacter.Speed -= 1;
            SpeedNumber.text = _customCharacter.Speed.ToString();
        }
    }
    public void DegradeCharacterResistance()
    {
        if (_customCharacter.Resistance > 0)
        {
            DiamondResistanceAmount += ResistanceCostAmount;
            DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
            _customCharacter.Resistance -= 1;
            ResistanceNumber.text = _customCharacter.Resistance.ToString();
        }
    }
    public void DegradeCharacterAttack()
    {
        if (_customCharacter.Attack > 0)
        {
            DiamondAttackAmount += AttackCostAmount;
            DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
            _customCharacter.Attack -= 1;
            AttackNumber.text = _customCharacter.Attack.ToString();
        }
    }
    public void DegradeCharacterDefence()
    {
        if (_customCharacter.Defence > 0)
        {
            DiamondDefenceAmount += DefenceCostAmount;
            DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();
            _customCharacter.Defence -= 1;
            DefenceNumber.text = _customCharacter.Defence.ToString();
        }
    }



    // set at start
    public void SetCharacterStats()
    {
        CharacterName.text = _customCharacter.Name;

        SpeedNumber.text = _customCharacter.Speed.ToString();
        ResistanceNumber.text = _customCharacter.Resistance.ToString();
        AttackNumber.text = _customCharacter.Attack.ToString();
        DefenceNumber.text = _customCharacter.Defence.ToString();

        DiamondSpeedAmountNumber.text = DiamondSpeedAmount.ToString();
        DiamondResistanceAmountNumber.text = DiamondResistanceAmount.ToString();
        DiamondAttackAmountNumber.text = DiamondAttackAmount.ToString();
        DiamondDefenceAmountNumber.text = DiamondDefenceAmount.ToString();

        SpeedCostAmountNumber.text = SpeedCostAmount.ToString();
        ResistanceCostAmountNumber.text = ResistanceCostAmount.ToString();
        AttackCostAmountNumber.text = AttackCostAmount.ToString();
        DefenceCostAmountNumber.text = DefenceCostAmount.ToString();
    }

}
