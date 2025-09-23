using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/ArguerCharacter", fileName = "ArguerCharacterStats")]
    public class ArguerCharacter : ProjectorClassCharacter
    {
        public ArguerCharacter() : base()
        {
            _id = CharacterID.Arguer;
            _defaultAttack = 7;
            _defaultDefence = 10;
            _defaultHp = 3;
            _defaultCharacterSize = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
