using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class GraffitiartistCharacter : ProjectorClassCharacter
    {
        public GraffitiartistCharacter() : base()
        {
            _id = CharacterID.ProjectorGraffitiArtist;
            _defaultAttack = 7;
            _defaultDefence = 10;
            _defaultHp = 3;
            _defaultResistance = 8;
            _defaultSpeed = 4;
            InitilizeValues();
        }
    }
}
