using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/FashionSlaveCharacter", fileName = "FashionSlaveCharacterStats")]
    public class FashionSlaveCharacter : ObedientClassCharacter
    {
        public FashionSlaveCharacter() : base()
        {
            _id = CharacterID.FashionSlave;
            _defaultAttack = 4;
            _defaultDefence = 3;
            _defaultHp = 3;
            _defaultCharacterSize = 4;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
