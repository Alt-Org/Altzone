using System;
using System.Collections.Generic;
using System.Reflection;
using GameAnalyticsSDK.Setup;
using UnityEditor;
using UnityEngine;

namespace Prg.Editor.Data
{
    public static class AnalyticsSettings
    {
        private const string MenuRoot = "Prg/";
        private const string MenuItem = MenuRoot + "Game/";

        [MenuItem(MenuItem + "Test GameAnalytics Settings", false, 20)]
        private static void TestGameAnalyticsSettings()
        {
            Debug.Log("*");
            CreateForPlatform(BuildTarget.StandaloneWindows64, new Tuple<string, string>("gameKey_win", "secretKey_win"));
        }

        public static bool CreateForPlatform(BuildTarget buildTarget, Tuple<string, string> tuple)
        {
            var settings = Resources.Load<Settings>("GameAnalytics/Settings");
            var platforms = settings.Platforms;
            var platformIndex = buildTarget switch
            {
                BuildTarget.Android => platforms.FindIndex(x => x == RuntimePlatform.Android),
                BuildTarget.WebGL => platforms.FindIndex(x => x == RuntimePlatform.WebGLPlayer),
                BuildTarget.StandaloneWindows64 => platforms.FindIndex(x => x == RuntimePlatform.WindowsPlayer),
                _ => -1
            };
            if (platformIndex == -1)
            {
                Debug.Log($"{RichText.Red("Did not find Platform settings")} for BuildTarget {RichText.Yellow(buildTarget)}");
                return false;
            }
            Debug.Log($"BuildTarget {RichText.Yellow(buildTarget)}");
            return SetKeys(settings, platformIndex, tuple);
        }

        private static bool SetKeys(Settings settings, int index, Tuple<string, string> tuple)
        {
            // ReSharper disable EntityNameCapturedOnly.Local
            const string gameKey = "";
            const string secretKey = "";
            // ReSharper restore EntityNameCapturedOnly.Local

            var type = settings.GetType();
            var field1 = type.GetField(nameof(gameKey), BindingFlags.Instance | BindingFlags.NonPublic);
            var value1 = field1?.GetValue(settings);
            if (value1 is not List<string> gameKeys)
            {
                Debug.Log($"{nameof(gameKey)} not found in {settings}");
                return false;
            }
            var field2 = type.GetField(nameof(secretKey), BindingFlags.Instance | BindingFlags.NonPublic);
            var value2 = field2?.GetValue(settings);
            if (value2 is not List<string> secretKeys)
            {
                Debug.Log($"{nameof(secretKey)} not found in {settings}");
                return false;
            }
            var isDirty = false;
            var gameKeyValue = gameKeys[index];
            var secretKeyValue = secretKeys[index];
            if (gameKeyValue != tuple.Item1)
            {
                gameKeys[index] = tuple.Item1;
                isDirty = true;
                Debug.Log($"update {gameKeyValue} in {settings}");
            }
            if (secretKeyValue != tuple.Item2)
            {
                secretKeys[index] = tuple.Item2;
                isDirty = true;
                Debug.Log($"update {secretKeyValue} in {settings}");
            }
            if (!isDirty)
            {
                return false;
            }
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return true;
        }
    }
}
