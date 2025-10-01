using UnityEngine;

public class TopBarDefs
{
    private const string keyPrefix = "TopBarItem_";
    
    public enum TopBarItem
    {
        PlayerProfile,
        ClanLogo,
        Leaderboard,
        Coins,
        Settings,
        ClanTile,
    }

    public static string Key(TopBarItem item) => $"{keyPrefix}{item}";

}
