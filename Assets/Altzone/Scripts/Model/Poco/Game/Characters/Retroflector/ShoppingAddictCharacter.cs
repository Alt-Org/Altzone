using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/ShoppingAddictCharacter", fileName = "ShoppingAddictCharacterStats")]
    public class ShoppingAddictCharacter : RetroflectorClassCharacter
    {
        public ShoppingAddictCharacter() : base()
        {
            _id = CharacterID.ShoppingAddict;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
