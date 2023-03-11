using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RaidRoom
    {
        public int Id;
        public int ClanMemberId;
        public int GameFurnitureId;
        public RaidRoomType Type;
    }
}