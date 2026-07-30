namespace Altzone.Scripts.Settings
{
    public static class TopBarDefs
    {
        private const string KeyPrefix = "TopBarItem_";

        public enum TopBarItem
        {
            ClanTile,
            Leaderboard,
            Coins,
            ClanLogo,
            ClanTextContainer,
            Jukebox,
            PlayerProfile,
            Voting,
            Settings,
        }

        public static string Key(TopBarItem item) => $"{KeyPrefix}{item}";
    }
}
