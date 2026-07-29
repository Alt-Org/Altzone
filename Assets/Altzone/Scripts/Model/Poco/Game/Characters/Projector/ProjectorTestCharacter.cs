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
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 6;
            _defaultCharacterSize = 2;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
