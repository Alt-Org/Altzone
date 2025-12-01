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
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 6;
            _defaultCharacterSize = 2;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
