using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsWindow/New CharacterStatsWindow", menuName = "CharacterStatsWindow")]
public class CharacterStatsWindow : ScriptableObject
{
    public string CharacterName;
    public Sprite CharacterArtWork;
    public bool Favourite;

    public int DiamondSpeedAmount;
    public int DiamondResistanceAmount;
    public int DiamondAttackAmount;
    public int DiamondDefenceAmount;

    public int CharacterSpeed;
    public int CharacterResistance;
    public int CharacterAttack;
    public int CharacterDefence;

    public int SpeedCostAmount;
    public int ResistanceCostAmount;
    public int AttackCostAmount;
    public int DefenceCostAmount;
}
