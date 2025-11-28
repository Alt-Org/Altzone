using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/MaternalFixationCharacter", fileName = "MaternalFixationCharacterStats")]
    public class MaternalFixationCharacter : ObedientClassCharacter
    {
        public MaternalFixationCharacter() : base()
        {
            _id = CharacterID.MaternalFixation;
            _defaultAttack = 10;
            _defaultDefence = 10;
            _defaultHp = 10;
            _defaultCharacterSize = 10;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
