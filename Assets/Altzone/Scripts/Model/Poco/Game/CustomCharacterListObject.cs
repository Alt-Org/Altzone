using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [Serializable]
    public class CustomCharacterListObject
    {
        public string ServerID { get; set; }
        public CharacterID CharacterID { get; set; }
        public bool IsTestCharacter { get; set; }

        public CustomCharacterListObject(string serverId = null, CharacterID Id = CharacterID.None)
        {
            if (serverId != null) ServerID = serverId;
            else ServerID = null;
            CharacterID = Id;
            if(CustomCharacter.IsTestCharacter(CharacterID)) IsTestCharacter = true;
            else IsTestCharacter = false;
        }

        public void SetData(string serverId = null, CharacterID Id = CharacterID.None)
        {
            if (serverId != null) ServerID = serverId;
            else ServerID = null;
            CharacterID = Id;
            if (CustomCharacter.IsTestCharacter(CharacterID)) IsTestCharacter = true;
            else IsTestCharacter = false;
        }

        public override string ToString()
        {
            return
                $"{nameof(ServerID)}: {ServerID}, {nameof(CharacterID)}: {CharacterID}, {nameof(IsTestCharacter)}: {IsTestCharacter}";
        }
    }
}
