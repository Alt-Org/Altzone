using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/BodybuilderCharacter", fileName = "BodybuilderCharacterStats")]
    public class BodybuilderCharacter : DesentisitizerClassCharacter
    {
        public BodybuilderCharacter()
        {
            _id = CharacterID.Bodybuilder;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
