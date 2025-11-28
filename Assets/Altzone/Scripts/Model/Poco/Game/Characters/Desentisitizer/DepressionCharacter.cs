using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/DepressionCharacter", fileName = "DepressionCharacterStats")]
    public class DepressionCharacter : DesentisitizerClassCharacter
    {
        public DepressionCharacter()
        {
            _id = CharacterID.Depression;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
