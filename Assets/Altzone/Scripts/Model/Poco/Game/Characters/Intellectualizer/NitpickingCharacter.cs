using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/NitpickingCharacter", fileName = "NitpickingCharacterStats")]
    public class NitpickingCharacter : IntellectualizerClassCharacter
    {
        public NitpickingCharacter() : base()
        {
            _id = CharacterID.Nitpicking;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
