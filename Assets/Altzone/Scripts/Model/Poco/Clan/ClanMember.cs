using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Player;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanMember
    {
        [PrimaryKey] public string Id;
        [ForeignKey(nameof(PlayerData)), Mandatory] public string PlayerDataId;
        [ForeignKey(nameof(RaidRoom)), Optional] public string RaidRoomId;
        public ClanMemberRole Role;
    }
}