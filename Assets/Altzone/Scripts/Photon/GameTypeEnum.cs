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
        BattleTestFlipperGame = 1,
        Raid = 10,
    }
    public static class CustomGameModeExtension
    {
        public static string GetString(this GameType gameMode)
        {
            return SettingsCarrier.Instance.Language switch
            {
                SettingsCarrier.LanguageType.English => gameMode switch
                {
                    GameType.BattlePingPong => "Basegame",
                    GameType.BattleTestFlipperGame => "Flipper test",
                    GameType.Raid => "Raid",
                    _ => ""
                },
                SettingsCarrier.LanguageType.Finnish => gameMode switch
                {
                    GameType.BattlePingPong => "Peruspeli",
                    GameType.BattleTestFlipperGame => "Flipper testi",
                    GameType.Raid => "Ryöstö",
                    _ => ""
                },
                _ => gameMode switch
                {
                    GameType.BattlePingPong => "Peruspeli",
                    GameType.BattleTestFlipperGame => "Flipper testi",
                    GameType.Raid => "Ryöstö",
                    _ => ""
                },
            };
        }
    }
}

