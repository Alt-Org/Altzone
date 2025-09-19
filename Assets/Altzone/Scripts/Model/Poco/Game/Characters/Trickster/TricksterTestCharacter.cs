using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/TricksterTestCharacter", fileName = "TricksterTestCharacterStats")]
    public class TricksterTestCharacter : TricksterClassCharacter
    {
        public TricksterTestCharacter()
        {
            _id = CharacterID.TricksterTest;
            _defaultAttack = 8;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultCharacterSize = 4;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
