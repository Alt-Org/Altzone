using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class DesentisitizerClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Desensitizer;


        protected DesentisitizerClassCharacter()
        {
            InitilizeValues();
        }
    }
}
