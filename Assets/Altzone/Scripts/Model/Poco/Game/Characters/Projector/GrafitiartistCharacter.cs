using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class GraffitiartistCharacter : ProjectorClassCharacter
    {
        public GraffitiartistCharacter() : base()
        {
            _id = CharacterID.ProjectorGrafitiartist;
            _defaultAttack = 2;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultResistance = 1;
            _speed = 3;
            InitilizeValues();
        }
    }
}
