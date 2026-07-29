using System;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, Flags]
    public enum ClanMemberRole
    {
        None = 0,
        Member = 1,
        Officer = 2,
        Admin = 3
    }
}
