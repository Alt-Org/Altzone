using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Prefabs.Windows.DefenceScreen
{

    public class DemoCharacterWindowCharacter
    {
        public string CharacterName;
        public bool Favourite;

        public int CharacterSpeed;
        public int CharacterResistance;
        public int CharacterAttack;
        public int CharacterDefence;
        public int CharacterHP;

        public DemoCharacterWindowCharacter(string name, bool favourite, int speed, int resistance, int attack, int defence, int hp)
        {
            CharacterName = name;
            Favourite = favourite;
            CharacterSpeed = speed;
            CharacterResistance = resistance;
            CharacterAttack = attack;
            CharacterDefence = defence;
            CharacterHP = hp;
        }
    }
}
