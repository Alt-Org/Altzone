using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class TricksterClassCharacter : BaseCharacter
    {
        public override CharacterClassID ClassID => CharacterClassID.Trickster;


        protected TricksterClassCharacter()
        {
            InitilizeValues();
        }
    }
}
