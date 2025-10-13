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
            _defaultAttack = 2;
            _defaultDefence = 8;
            _defaultHp = 3;
            _defaultCharacterSize = 6;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
