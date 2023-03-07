using UnityEngine;

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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            Model.Store.Storefront.Instance = null;
        }

        public static IStorefront Get()
        {
            return Model.Store.Storefront.Instance ??= new Model.Store.Storefront();
        }
    }
}