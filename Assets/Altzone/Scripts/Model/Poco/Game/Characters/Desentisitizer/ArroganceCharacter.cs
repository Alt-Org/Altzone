using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/ArroganceCharacter", fileName = "ArroganceCharacterStats")]
    public class ArroganceCharacter : DesentisitizerClassCharacter
    {
        public ArroganceCharacter()
        {
            _id = CharacterID.Arrogance;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
