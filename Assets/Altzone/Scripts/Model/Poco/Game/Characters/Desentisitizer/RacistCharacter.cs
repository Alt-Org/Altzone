using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/RacistCharacter", fileName = "RacistCharacterStats")]
    public class RacistCharacter : DesentisitizerClassCharacter
    {
        public RacistCharacter()
        {
            _id = CharacterID.Racist;
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
