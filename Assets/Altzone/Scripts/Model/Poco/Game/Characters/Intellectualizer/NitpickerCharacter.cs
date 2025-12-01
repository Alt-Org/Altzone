using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/NitpickerCharacter", fileName = "NitpickerCharacterStats")]
    public class NitpickerCharacter : IntellectualizerClassCharacter
    {
        public NitpickerCharacter() : base()
        {
            _id = CharacterID.Nitpicker;
            _defaultAttack = 2;
            _defaultDefence = 1;
            _defaultHp = 3;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
