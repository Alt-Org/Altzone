using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/BodybuilderCharacter", fileName = "BodybuilderCharacterStats")]
    public class BodybuilderCharacter : DesentisitizerClassCharacter
    {
        public BodybuilderCharacter()
        {
            _id = CharacterID.Bodybuilder;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
