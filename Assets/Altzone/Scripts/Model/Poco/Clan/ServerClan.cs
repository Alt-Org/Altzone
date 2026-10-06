using System;
using System.Collections.Generic;
using Altzone.Scripts.Voting;

/// <summary>
/// Clan object received from the server
/// </summary>
namespace Altzone.Scripts.Model.Poco.Clan
{
    public class ServerClan
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string tag { get; set; }
        public string phrase { get; set; }
        public List<string> labels { get; set; }
        public int gameCoins { get; set; }
        public int points { get; set; }
        public List<string> admin_ids { get; set; }
        public int playerCount { get; set; }
        public int itemCount { get; set; }
        public int stockCount { get; set; }
        public ClanAge ageRange { get; set; }
        public Language language { get; set; }
        public Goals goal { get; set; }
        public bool isOpen { get; set; }
        public int? furnitureCount { get; set; }
        public int? raidRoomCount { get; set; }
        public List<PollData> polls { get; set; }
        public ClanLogo clanLogo { get; set; }
        public List<ClanRoles> roles { get; set; }
    }

    public enum ClanLogoType
    {
        None,
        Heart
    }
    [Serializable]
    public class ClanLogo
    {
        public ClanLogoType logoType { get; set; }
        public List<string> pieceColors { get; set; }
    }

    public enum ClanRoleType
    {
        None,
        Default,
        Named
    }

    public enum PlayerRights
    {
        CanEditHome,
        CanEditClanData,
        CanEditRights,
        CanManagerRoles,
        CanManageShop
    }

    [Serializable]
    public class ClanRoles
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string clanRoleType { get; set; }
        public ClanRights rights { get; set; }

        [Serializable]
        public class ClanRights
        {
            public bool edit_soulhome { get; set; }
            public bool edit_clan_data { get; set; }
            public bool edit_member_rights { get; set; }
            public bool manage_role { get; set; }
            public bool shop { get; set; }
        }

        public bool HasRights(PlayerRights right)
        {
            if (rights == null) return false;

            return right switch
            {
                PlayerRights.CanEditHome => rights.edit_soulhome,
                PlayerRights.CanEditClanData => rights.edit_clan_data,
                PlayerRights.CanEditRights => rights.edit_member_rights,
                PlayerRights.CanManagerRoles => rights.manage_role,
                PlayerRights.CanManageShop => rights.shop,
                _ => false
            };
        }

        public void SetRights(PlayerRights right, bool value)
        {
            if (rights == null) rights = new ClanRights();

            switch (right)
            {
                case PlayerRights.CanEditHome:
                    rights.edit_soulhome = value;
                    break;
                case PlayerRights.CanEditClanData:
                    rights.edit_clan_data = value;
                    break;
                case PlayerRights.CanEditRights:
                    rights.edit_member_rights = value;
                    break;
                case PlayerRights.CanManagerRoles:
                    rights.manage_role = value;
                    break;
                case PlayerRights.CanManageShop:
                    rights.shop = value;
                    break;
            }
        }
    }
}
