using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/Characters/GraffitiartistCharacter", fileName = "GraffitiartistCharacterStats")]
    public class GraffitiartistCharacter : ProjectorClassCharacter
    {
        public GraffitiartistCharacter() : base()
        {
            _id = CharacterID.Artist;
            _defaultAttack = 7;
            _defaultDefence = 10;
            _defaultHp = 3;
            _defaultCharacterSize = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
