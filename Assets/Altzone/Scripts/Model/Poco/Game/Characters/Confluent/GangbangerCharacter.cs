using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/GangbangerCharacter", fileName = "GangbangerCharacterStats")]
    public class GangbangerCharacter : ConfluentClassCharacter
    {
        public GangbangerCharacter() : base()
        {
            _id = CharacterID.GangBanger;
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
