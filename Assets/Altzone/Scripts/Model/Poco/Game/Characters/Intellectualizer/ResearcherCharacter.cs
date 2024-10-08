using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class ResearcherCharacter : IntellectualizerClassCharacter
    {
        public ResearcherCharacter() : base()
        {
            _id = CharacterID.IntellectualizerResearcher;
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultResistance = 3;
            _speed = 4;
            InitilizeValues();
        }
    }
}
