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
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 2;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
