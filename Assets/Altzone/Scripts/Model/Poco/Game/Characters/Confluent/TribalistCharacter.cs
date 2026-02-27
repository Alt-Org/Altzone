using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TribalistCharacter", fileName = "TribalistCharacterStats")]
    public class TribalistCharacter : ConfluentClassCharacter
    {
        public TribalistCharacter() : base()
        {
            _id = CharacterID.Tribalist;
            _defaultAttack = 1;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
