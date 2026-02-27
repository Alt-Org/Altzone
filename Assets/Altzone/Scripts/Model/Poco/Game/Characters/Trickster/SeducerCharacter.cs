using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/SeducerCharacter", fileName = "SeducerCharacterStats")]
    public class SeducerCharacter : TricksterClassCharacter
    {
        public SeducerCharacter()
        {
            _id = CharacterID.Seducer;
            _defaultAttack = 1;
            _defaultDefence = 3;
            _defaultHp = 2;
            _defaultCharacterSize = 2;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
