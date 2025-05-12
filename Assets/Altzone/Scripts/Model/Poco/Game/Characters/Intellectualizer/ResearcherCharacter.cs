using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ResearcherCharacter", fileName = "ResearcherCharacterStats")]
    public class ResearcherCharacter : IntellectualizerClassCharacter
    {
        public ResearcherCharacter() : base()
        {
            _id = CharacterID.Booksmart;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
