using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;

namespace MenuUI.Scripts.SoulHome
{
    [System.Serializable]
    public class SoulHome
    {
        public int Id;
        public ClanMemberRole editPermission;
        public ClanMemberRole addRemovePermission;
        public List<Room> Room;
    }
}
