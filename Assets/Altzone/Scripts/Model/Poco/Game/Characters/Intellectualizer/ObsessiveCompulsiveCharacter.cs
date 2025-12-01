using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/ObsessiveCompulsiveCharacter", fileName = "ObsessiveCompulsiveCharacterStats")]
    public class ObsessiveCompulsiveCharacter : IntellectualizerClassCharacter
    {
        public ObsessiveCompulsiveCharacter() : base()
        {
            _id = CharacterID.ObsessiveCompulsive;
            _defaultAttack = 2;
            _defaultDefence = 1;
            _defaultHp = 3;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
