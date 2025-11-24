using System;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Player
{
    [Serializable]
    public class TeamLoadOut
    {
        public CustomCharacterListObject[] Slots = new CustomCharacterListObject[3]
        {
            new CustomCharacterListObject(Id: CharacterID.None),
            new CustomCharacterListObject(Id: CharacterID.None),
            new CustomCharacterListObject(Id: CharacterID.None)
        };

        public bool IsEmpty
        {
            get
            {
                if (Slots == null) return true;
                foreach (var slot in Slots)
                {
                    if (slot != null && slot.CharacterID != CharacterID.None)
                        return false;
                }
                return true;
            }
        }
    }
}
