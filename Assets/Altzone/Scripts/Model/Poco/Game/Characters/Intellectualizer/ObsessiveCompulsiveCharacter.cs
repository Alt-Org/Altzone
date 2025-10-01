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
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
