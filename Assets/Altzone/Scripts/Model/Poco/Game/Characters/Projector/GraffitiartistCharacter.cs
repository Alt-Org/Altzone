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
            _defaultAttack = 2;
            _defaultDefence = 3;
            _defaultHp = 6;
            _defaultCharacterSize = 3;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
