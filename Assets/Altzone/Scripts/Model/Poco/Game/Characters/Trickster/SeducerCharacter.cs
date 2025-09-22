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
            _defaultAttack = 8;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultCharacterSize = 4;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
