using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        public string Id;
        public string PlayerDataId;
        public string RaidRoomId;
        public ClanMemberRole Role;
    }
}