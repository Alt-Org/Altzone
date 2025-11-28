using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/SelfharmCharacter", fileName = "SelfharmCharacterStats")]
    public class SelfharmCharacter : RetroflectorClassCharacter
    {
        public SelfharmCharacter() : base()
        {
            _id = CharacterID.SelfHarm;
            _defaultAttack = 6;
            _defaultDefence = 8;
            _defaultHp = 1;
            _defaultCharacterSize = 12;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
