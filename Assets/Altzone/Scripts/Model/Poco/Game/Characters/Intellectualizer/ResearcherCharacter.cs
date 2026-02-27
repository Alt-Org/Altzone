using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/Characters/ResearcherCharacter", fileName = "ResearcherCharacterStats")]
    public class ResearcherCharacter : IntellectualizerClassCharacter
    {
        public ResearcherCharacter() : base()
        {
            _id = CharacterID.Booksmart;
            _defaultAttack = 3;
            _defaultDefence = 1;
            _defaultHp = 2;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
