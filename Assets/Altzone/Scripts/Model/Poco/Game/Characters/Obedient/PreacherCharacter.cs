using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/Characters/PreacherCharacter", fileName = "PreacherCharacterStats")]
    public class PreacherCharacter : ObedientClassCharacter
    {
        public PreacherCharacter() : base()
        {
            _id = CharacterID.Religious;
            _defaultAttack = 4;
            _defaultDefence = 4;
            _defaultHp = 3;
            _defaultCharacterSize = 4;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
