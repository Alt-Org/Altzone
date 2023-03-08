using Editor.Prg.Build;
using Editor.Prg.Dependencies;
using Editor.Prg.GameDebug;
using Editor.Prg.Localization;
using Editor.Prg.Logging;
using Editor.Prg.Util;
using UnityEditor;

namespace Editor
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
        private const string Localization = MenuRoot + "Localization/";
        private const string Dependencies = MenuRoot + "Dependencies/";
        private const string MissingReferences = Dependencies + "Missing References : ";
        private const string Build = MenuRoot + "Build/";
        private const string Logging = MenuRoot + "Logging/";
        private const string Util = MenuRoot + "Util/";

        #region GameDebug

        [MenuItem(GameDebug + "Show Player Settings", false, 10)]
        private static void ShowLocalPlayerSettings() => DebugMenu.ShowLocalPlayerSettings();

        [MenuItem(GameDebug + "Create Dummy Player Settings", false, 11)]
        private static void CreateDummyPlayerSettings() => DebugMenu.CreateDummyPlayerSettings();

        [MenuItem(GameDebug + "Reset Player Settings", false, 12)]
        private static void ResetPlayerSettings() => DebugMenu.ResetPlayerSettings();

        [MenuItem(GameDebug + "Set Player Language to 'EN'", false, 13)]
        private static void SetLanguageToEn() => DebugMenu.SetLanguageToEn();

        #endregion

        #region Localization

        [MenuItem(Localization + "Load Translations (bin)", false, 10)]
        private static void LoadTranslations() => LocalizerMenu.LoadTranslations();

        [MenuItem(Localization + "Save Translations (tsv->bin)", false, 11)]
        private static void SaveTranslations() => LocalizerMenu.SaveTranslations();

        [MenuItem(Localization + "Show Translations (bin)", false, 12)]
        private static void ShowTranslations() => LocalizerMenu.ShowTranslations();

        [MenuItem(Localization + "Check Selected Asset(s)", false, 13)]
        private static void CheckUsedTranslationsInAssets() => LocalizerMenu.CheckUsedTranslationsInAssets();

        [MenuItem(Localization + "Open In Google Drive", false, 14)]
        private static void OpenInGoogleDrive() => LocalizerMenu.OpenInGoogleDrive();

        [MenuItem(Localization + "Show Localization Window", false, 14)]
        private static void SearchLocalizationKeys() => SmartTextSearchEditorWindow.SearchLocalizationKeys();

        #endregion

        #region Dependencies

        [MenuItem(Dependencies + "Check Usages", false, 10)]
        private static void CheckUsages() => CheckDependencies.CheckUsages();

        [MenuItem(Dependencies + "Show Folders", false, 11)]
        private static void ShowFolders() => CheckDependencies.ShowFolders();

        [MenuItem(Dependencies + "Sort Selection", false, 12)]
        private static void SortSelection() => CheckDependencies.SortSelection();

        [MenuItem(Dependencies + "Check for Missing References", false, 13)]
        private static void CheckDeletedGuids() => CheckDependencies.CheckMissingReferences();

        [MenuItem(Dependencies + "Check for Unused References", false, 14)]
        private static void CheckUnusedReferences() => CheckDependencies.CheckUnusedReferences();

        [MenuItem(MissingReferences + "Search in scene", false, 20)]
        private static void FindMissingReferencesInCurrentScene() => MissingReferencesFinder.FindMissingReferencesInCurrentScene();

        [MenuItem(MissingReferences + "Search in all scenes", false, 21)]
        private static void FindMissingReferencesInAllScenes() => MissingReferencesFinder.FindMissingReferencesInAllScenes();

        [MenuItem(MissingReferences + "Search in assets", false, 22)]
        private static void FindMissingReferencesInAssets() => MissingReferencesFinder.FindMissingReferencesInAssets();

        [MenuItem(MissingReferences + "Search in selection", false, 23)]
        private static void FindMissingReferencesInSelection() => MissingReferencesFinder.FindMissingReferencesInSelection();

        [MenuItem(Dependencies + "Force Update Local Asset History", false, 24)]
        private static void UpdateAssetHistoryMenu() => AssetHistoryUpdater.UpdateAssetHistory();

        #endregion

        #region Util

        [MenuItem(Util + "List Used layers in Scene", false, 10)]
        private static void ListUsedLayersInScene() => ListUsedLayers.ListUsedLayersInScene();

        [MenuItem(Util + "List Used tags in Scene", false, 11)]
        private static void ListUsedTagsInScene() => ListUsedLayers.ListUsedTagsInScene();

        [MenuItem(Util + "List GameObjects with layer or tag in Scene", false, 12)]
        private static void ListGameObjectsWithLayerOrTagInScene() => ListUsedLayers.ListGameObjectsWithLayerOrTagInScene();

        [MenuItem(Util + "Create Project 'Standard' Folders", false, 13)]
        private static void CreateProjectStandardFolders() => CreateStandardFolders.CreateProjectStandardFolders();

        [MenuItem(Util + "Generate UnityConstants.cs", false, 14)]
        private static void GenerateUnityConstants() => UnityConstantsGenerator.GenerateUnityConstants();

        [MenuItem(Util + "Show Version Info", false, 15)]
        private static void ShowVersionInfo() => MenuShowVersionInfo.ShowVersionInfo();

        [MenuItem(Util + "Set Label 'Altzone'", false, 16)]
        private static void SetLabelAltzone() => CheckDependencies.SetLabel("Altzone");

        #endregion

        #region Logging

        [MenuItem(Logging + "Add 'FORCE_LOG' define", false, 10)]
        private static void AddDefine() => LoggerMenu.AddDefine();

        [MenuItem(Logging + "Remove 'FORCE_LOG' define", false, 11)]
        private static void RemoveDefine() => LoggerMenu.RemoveDefine();

        [MenuItem(Logging + "Highlight logging settings", false, 12)]
        private static void HighlightLoggerSettings() => LoggerMenu.HighlightLoggerSettings();

        [MenuItem(Logging + "Show log file location", false, 13)]
        private static void ShowLogFilePath() => LoggerMenu.ShowLogFilePath();

        [MenuItem(Logging + "Open log file in text editor", false, 14)]
        private static void LoadLogFileToTextEditor() => LoggerMenu.LoadLogFileToTextEditor();

        #endregion

        #region Build

        [MenuItem(Build + "Create Build Report", false, 10)]
        private static void CheckBuildReport() => MenuBuildReport.CheckBuildReport();

        [MenuItem(Build + "Create Build Script", false, 11)]
        private static void CreateBuildScript() => MenuBuildReport.CreateBuildScript();

        [MenuItem(Build + "Test Android Build Config", false, 12)]
        private static void CheckAndroidBuild() => MenuBuildReport.CheckAndroidBuild();

        [MenuItem(Build + "Set Android Build for Local APK Test", false, 13)]
        private static void SetAndroidBuildTestApk() => MenuBuildReport.SetAndroidBuildTestApk();

        #endregion
    }
}