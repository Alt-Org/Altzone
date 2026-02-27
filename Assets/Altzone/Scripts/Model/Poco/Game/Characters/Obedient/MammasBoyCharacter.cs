using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/MammasBoyCharacter", fileName = "MammasBoyCharacterStats")]
    public class MammasBoyCharacter : ObedientClassCharacter
    {
        public MammasBoyCharacter() : base()
        {
            _id = CharacterID.MammasBoy;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 4;
            _defaultCharacterSize = 4;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
