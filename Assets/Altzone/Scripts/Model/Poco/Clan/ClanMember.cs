using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Player;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        public string Id;

        [ForeignKeyReference(nameof(PlayerData))]
        public string PlayerDataId;

        [ForeignKeyReference(nameof(RaidRoom))]
        public string RaidRoomId;

        public ClanMemberRole Role;
    }
}