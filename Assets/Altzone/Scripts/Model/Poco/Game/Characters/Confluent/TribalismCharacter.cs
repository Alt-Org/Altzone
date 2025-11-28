using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TribalismCharacter", fileName = "TribalismCharacterStats")]
    public class TribalismCharacter : ConfluentClassCharacter
    {
        public TribalismCharacter() : base()
        {
            _id = CharacterID.Tribalism;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
