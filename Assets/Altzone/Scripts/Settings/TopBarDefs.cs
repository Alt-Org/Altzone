namespace Altzone.Scripts.Settings
{
    public static class TopBarDefs
    {
        private const string KeyPrefix = "TopBarItem_";

        public enum TopBarItem
        {
            PlayerProfile,
            ClanLogo,
            Leaderboard,
            Coins,
            Settings,
            ClanTile,
            Jukebox,
            ClanTextContainer,
            Voting,
        }

        public static string Key(TopBarItem item) => $"{KeyPrefix}{item}";
    }
}
