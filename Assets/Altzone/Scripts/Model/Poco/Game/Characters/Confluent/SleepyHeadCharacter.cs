using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class SleepyHeadCharacter : ConfluentClassCharacter
    {
        public SleepyHeadCharacter() : base()
        {
            _id = CharacterID.SleepyHead;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
