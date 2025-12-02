using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/OvercompilatorCharacter", fileName = "OvercompilatorCharacterStats")]
    public class OvercompilatorCharacter : IntellectualizerClassCharacter
    {
        public OvercompilatorCharacter() : base()
        {
            _id = CharacterID.Overcompilator;
            _defaultAttack = 2;
            _defaultDefence = 1;
            _defaultHp = 3;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
