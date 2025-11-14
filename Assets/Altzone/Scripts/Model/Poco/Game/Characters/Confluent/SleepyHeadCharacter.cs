using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/SleepyHeadCharacter", fileName = "SleepyHeadCharacterStats")]
    public class SleepyHeadCharacter : ConfluentClassCharacter
    {
        public SleepyHeadCharacter() : base()
        {
            _id = CharacterID.SleepyHead;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
