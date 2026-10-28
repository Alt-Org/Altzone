namespace Altzone.Scripts.Lobby
{
    /// <summary>
    /// Used for determining battle popup game type.
    /// </summary>
    public enum MatchmakingType
    {
        None = -1,
        Custom = 0,
        Random2v2 = 1,
        Clan2v2 = 2,
        FriendLobby = 3,
    }

    public enum GameType
    {
        None = -1,
        BattlePingPong = 0,
        BattleFlipperGame = 1,
        Raid = 10,
    }

    public enum PlayerCount
    {
        Two = 2,
        Four = 4
    }

    public static class CustomGameModeExtension
    {
        public static string GetString(this GameType gameMode)
        {
            return Localization.Language switch
            {
                LanguageType.English => gameMode switch
                {
                    GameType.BattlePingPong => "Basegame",
                    GameType.BattleFlipperGame => "Flipper",
                    GameType.Raid => "Raid",
                    _ => ""
                },
                LanguageType.Finnish => gameMode switch
                {
                    GameType.BattlePingPong => "Peruspeli",
                    GameType.BattleFlipperGame => "Flipperi",
                    GameType.Raid => "Ryöstö",
                    _ => ""
                },
                _ => gameMode switch
                {
                    GameType.BattlePingPong => "Peruspeli",
                    GameType.BattleFlipperGame => "Flipperi",
                    GameType.Raid => "Ryöstö",
                    _ => ""
                },
            };
        }

        public static string GetString(this PlayerCount gameMode)
        {
            return Localization.Language switch
            {
                LanguageType.English => gameMode switch
                {
                    PlayerCount.Two => "1v1",
                    PlayerCount.Four => "2v2",
                    _ => ""
                },
                LanguageType.Finnish => gameMode switch
                {
                    PlayerCount.Two => "1v1",
                    PlayerCount.Four => "2v2",
                    _ => ""
                },
                _ => gameMode switch
                {
                    PlayerCount.Two => "1v1",
                    PlayerCount.Four => "2v2",
                    _ => ""
                },
            };
        }
    }
}

