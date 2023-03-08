namespace Altzone.Scripts
{
    /// <summary>
    /// Hardcoded files names for local storage.
    /// </summary>
    public static class GameFiles
    {
        public const string ClanGameRoomModelsFilename = "GameClanGameRoomModels.json";
        public const string ClanInventoryItemsFilename = "GameClanInventoryItems.json";
        public const string PlayerGameRoomModelsFilename = "GamePlayerClanGameRoomModels.json";
        public const string PlayerCustomCharacterModelsFilename = "GamePlayerCustomCharacterModels.json";
    }

    /// <summary>
    /// Factory class for <c>IStorefront</c>.
    /// </summary>
    public static class Storefront
    {
        /// <summary>
        /// Gets or creates an <c>IStorefront</c> static instance. 
        /// </summary>
        public static IStorefront Get() => Model.Store.StorefrontAsync.Get();
    }
}