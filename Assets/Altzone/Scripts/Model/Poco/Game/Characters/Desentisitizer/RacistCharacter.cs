using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/RacistCharacter", fileName = "RacistCharacterStats")]
    public class RacistCharacter : DesentisitizerClassCharacter
    {
        public RacistCharacter()
        {
            _id = CharacterID.Racist;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
