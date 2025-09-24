using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/ProjectorTestCharacter", fileName = "ProjectorTestCharacterStats")]
    public class ProjectorTestCharacter : ProjectorClassCharacter
    {
        public ProjectorTestCharacter() : base()
        {
            _id = CharacterID.ProjectorTest;
            _defaultAttack = 7;
            _defaultDefence = 10;
            _defaultHp = 3;
            _defaultCharacterSize = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
