using System;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, Flags]
    public enum ClanRoleRights
    {
        None = 0,
        EditSoulHome = 1 << 0,
        EditClanSettings = 1 << 1, // 2
        EditMemberRights = 1 << 2, // 4
    }

    /// <summary>
    /// Extension class to handle bitwise operations on ClanRoleRights.
    /// </summary>
    public static class ClanRoleRightsExtensions
    {
        public static bool Contains(this ClanRoleRights currentRights, ClanRoleRights requiredRights) => (currentRights & requiredRights) == requiredRights;
        public static ClanRoleRights Add(this ClanRoleRights currentRights, ClanRoleRights newRights) => currentRights |= newRights;
        public static ClanRoleRights Remove(this ClanRoleRights currentRights, ClanRoleRights removedRights) => currentRights &= ~removedRights;
    }
}