using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/DelusionalCharacter", fileName = "DelusionalCharacterStats")]
    public class DelusionalCharacter : ProjectorClassCharacter
    {
        public DelusionalCharacter() : base()
        {
            _id = CharacterID.Delusional;
            _defaultAttack = 3;
            _defaultDefence = 2;
            _defaultHp = 6;
            _defaultCharacterSize = 2;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
