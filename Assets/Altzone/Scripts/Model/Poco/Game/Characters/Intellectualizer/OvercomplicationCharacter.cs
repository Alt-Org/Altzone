using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/OvercomplicationCharacter", fileName = "OvercomplicationCharacterStats")]
    public class OvercomplicationCharacter : IntellectualizerClassCharacter
    {
        public OvercomplicationCharacter() : base()
        {
            _id = CharacterID.Overcomplication;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
