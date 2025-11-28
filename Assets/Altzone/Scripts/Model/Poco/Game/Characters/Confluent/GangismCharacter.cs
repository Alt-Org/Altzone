using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/GangismCharacter", fileName = "GangismCharacterStats")]
    public class GangismCharacter : ConfluentClassCharacter
    {
        public GangismCharacter() : base()
        {
            _id = CharacterID.Gangism;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
