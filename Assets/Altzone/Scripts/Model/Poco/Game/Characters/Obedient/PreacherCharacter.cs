using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/PreacherCharacter", fileName = "PreacherCharacterStats")]
    public class PreacherCharacter : ObedientClassCharacter
    {
        public PreacherCharacter() : base()
        {
            _id = CharacterID.Religious;
            _defaultAttack = 10;
            _defaultDefence = 10;
            _defaultHp = 10;
            _defaultCharacterSize = 10;
            _defaultSpeed = 10;
            InitializeValues();
        }
    }
}
