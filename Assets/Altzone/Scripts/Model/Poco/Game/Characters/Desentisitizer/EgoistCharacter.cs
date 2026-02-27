using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/EgoistCharacter", fileName = "EgoistCharacterStats")]
    public class EgoistCharacter : DesentisitizerClassCharacter
    {
        public EgoistCharacter()
        {
            _id = CharacterID.Egoist;
            _defaultAttack = 2;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
