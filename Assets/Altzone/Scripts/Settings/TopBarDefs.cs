namespace Altzone.Scripts.Settings
{
    public static class TopBarDefs
    {
        private const string KeyPrefix = "TopBarItem_";

        public enum TopBarItem
        {
            Tile,
            Leaderboard,
            Coins,
            ClanLogo,
            ClanTextContainer,
            Jukebox,
            PlayerProfile,
            Voting,
            Settings,
            Tile2nd
        }

        public static string Key(TopBarItem item) => $"{KeyPrefix}{item}";
    }
}
