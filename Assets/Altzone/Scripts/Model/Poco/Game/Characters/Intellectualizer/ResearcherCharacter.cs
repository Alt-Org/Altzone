using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class ResearcherCharacter : IntellectualizerClassCharacter
    {
        public ResearcherCharacter() : base()
        {
            _id = CharacterID.Booksmart;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultResistance = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
