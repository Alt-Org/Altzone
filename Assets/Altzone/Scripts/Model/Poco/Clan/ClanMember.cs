using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        public int PlayerDataId;
        public int RaidRoomId;
        public ClanMemberRole Role;
    }
}