using Altzone.Editor.GameDebug;
using UnityEditor;

namespace Altzone.Editor
{
    /// <summary>
    /// Container to populate 'Window' menu items for this game in Editor. 
    /// </summary>
    internal static class WindowMenuItems
    {
        // https://riptutorial.com/unity3d/example/10475/menu-items
        // You can add a separator in between two menu items by making sure there is at least 10 digits in between the priority of the menu items.

        private const string MenuRoot = "Window/ALT-Zone/";
        private const string GameDebug = MenuRoot + "Game Debug/";
        private const string Language = GameDebug + "Language/";
        private const string PhotonRegion = GameDebug + "Photon Region/";

        #region GameDebug

        [MenuItem(GameDebug + "Show Player Settings", false, 10)]
        private static void ShowLocalPlayerSettings() => DebugMenu.ShowLocalPlayerSettings();

        [MenuItem(Language + "Finnish", false, 11)]
        private static void SetLanguageToFinnish() => DebugMenu.SetLanguageToFinnish();

        [MenuItem(Language + "English", false, 12)]
        private static void SetLanguageToEnglish() => DebugMenu.SetLanguageToEnglish();

        [MenuItem(GameDebug + "Reset Player Settings", false, 13)]
        private static void ResetPlayerSettings() => DebugMenu.ResetPlayerSettings();

        [MenuItem(PhotonRegion + "Default", false, 20)]
        private static void SetPhotonRegionToDefault() => DebugMenu.SetPhotonRegionToDefault();

        [MenuItem(PhotonRegion + "Europe", false, 21)]
        private static void SetPhotonRegionToEu() => DebugMenu.SetPhotonRegionToEu();

        [MenuItem(PhotonRegion + "USA West", false, 22)]
        private static void SetPhotonRegionToUs() => DebugMenu.SetPhotonRegionToUs();

        [MenuItem(PhotonRegion + "Asia", false, 23)]
        private static void SetPhotonRegionToAsia() => DebugMenu.SetPhotonRegionToAsia();
        
        #endregion
    }
}
