using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
#if USE_GA
using Prg.Editor.Data;
#endif

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Utility to build project with given settings (using .enf file) from command line.<br />
    /// https://docs.unity3d.com/Manual/EditorCommandLineArguments.html<br />
    /// Following arguments are mandatory:<br />
    /// -projectPath<br />
    /// -buildTarget<br />
    /// -logFile<br />
    /// -executeMethod Editor.Prg.BatchBuild.BatchBuild.BuildPlayer
    /// -envFile<br />
    /// </summary>
    /// <remarks>
    /// -envFile parameter is our own for our own build parameters in addition to required UNITY standard parameters.<br />
    /// '-executeMethod' parameter starts build process using this file.<br />
    /// </remarks>
    // ReSharper disable once UnusedType.Global
    internal static class BatchBuild
    {
        /// <summary>
        /// Called from command line or CI system to build the project 'executable'.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        internal static void BuildPlayer()
        {
            LogFileWriter.CreateLogFileWriter();
            _BuildPlayer();
        }

        /// <summary>
        /// Called from command line or CI system to post process build output after successful build.
        /// </summary>
        /// <remarks>
        /// We can not access UNITY build log file during the build itself
        /// so we must handle it in separate step after the build has been fully completed.<br />
        /// </remarks>
        // ReSharper disable once UnusedMember.Global
        internal static void BuildPlayerPostProcessing()
        {
            LogFileWriter.CreateLogFileWriter();
            _BuildPlayerPostProcessing();
        }

        private static void _BuildPlayer()
        {
            var unityVersion = Application.unityVersion;
            Debug.Log($"batch_build_ start BUILD in UNITY {unityVersion}");
            var options = new BatchBuildOptions(Environment.GetCommandLineArgs());
            Debug.Log($"batch_build_ options {options}");
            Debug.Log($"batch_build_ LogFile {options.LogFile}");
            if (!VerifyUnityVersionForBuild(unityVersion, out var editorVersion))
            {
                Debug.Log(
                    $"batch_build_ UNITY version {unityVersion} does not match last Editor version {editorVersion}");
                EditorApplication.Exit(2);
                return;
            }
            var timer = new Timer();
#if USE_GA
            var dataAnalytics = options.GameAnalytics;
            Debug.Log($"batch_build_ SET dataAnalytics.gameKey: {FormatUtil.PasswordToLog(dataAnalytics.gameKey)}");
            Debug.Log($"batch_build_ SET dataAnalytics.secretKey: {FormatUtil.PasswordToLog(dataAnalytics.secretKey)}");
            var changed = AnalyticsSettings.CreateForPlatform(
                options.BuildTarget, new Tuple<string, string>(dataAnalytics.gameKey, dataAnalytics.secretKey));
            Debug.Log($"batch_build_ dataAnalytics settings changed: {changed}");
#endif
            if (options.IsTestRun)
            {
                Debug.Log("batch_build_ IsTestRun build exit 0");
                EditorApplication.Exit(0);
                return;
            }
            if (!options.IsRebuild)
            {
                BuildInfoUpdater.UpdateFile(PlayerSettings.Android.bundleVersionCode);
            }
            var buildReport = BuildPLayer(options);
            var buildResult = buildReport.summary.result;
            if (options.IsBuildReport && buildResult == BuildResult.Succeeded)
            {
                var tsvReport = SafeReplaceFileExtension(options.LogFile, ".log", ".report.tsv");
                var jsReport = SafeReplaceFileExtension(options.LogFile, ".log", ".report.js");
                Debug.Log($"batch_build_ save tsvReport {tsvReport}");
                Debug.Log($"batch_build_ save jsReport {jsReport}");
                BatchBuildReport.SaveBuildReport(buildReport, tsvReport, jsReport);
            }
            timer.Stop();
            Debug.Log($"batch_build_ exit result {buildResult} time {timer.ElapsedTime}");
            Debug.Log($"batch_build_ Build Report" +
                      $"\tStarted\t{buildReport.summary.buildStartedAt:yyyy-dd-MM HH:mm}" +
                      $"\tEnded\t{buildReport.summary.buildEndedAt:yyyy-dd-MM HH:mm}");
            EditorApplication.Exit(buildResult == BuildResult.Succeeded ? 0 : 1);
        }

        private static void _BuildPlayerPostProcessing()
        {
            LogFileWriter.CreateLogFileWriter();
            Debug.Log($"batch_build_ start POST PROCESS in UNITY {Application.unityVersion}");
            var options = new BatchBuildOptions(Environment.GetCommandLineArgs());
            Debug.Log($"batch_build_ Options {options}");
            if (options.IsTestRun)
            {
                Debug.Log("batch_build_ IsTestRun build exit 0");
                EditorApplication.Exit(0);
                return;
            }
            var timer = new Timer();
            // Load build report.
            var jsReport = SafeReplaceFileExtension(options.LogFile, ".log", ".report.js");
            var buildReportAssets = BatchBuildReport.LoadFromFile(jsReport);
            // Create log file data.
            var tsvOutput = SafeReplaceFileExtension(options.LogFile, ".log", ".log.tsv");
            var jsOutput = SafeReplaceFileExtension(options.LogFile, ".log", ".log.js");
            var buildReportLog = BatchBuildLog.SaveBuildReportLog(options.LogFilePost, tsvOutput, jsOutput);
            if (buildReportLog == null)
            {
                timer.Stop();
                Debug.Log($"batch_build_ player data was not rebuilt - no data to report on {options.LogFile}");
                Debug.Log($"batch_build_ exit time {timer.ElapsedTime}");
                EditorApplication.Exit(10);
                return;
            }
            // Create project files list.
            var tsvFiles = SafeReplaceFileExtension(options.LogFile, ".log", ".files.tsv");
            var jsFiles = SafeReplaceFileExtension(options.LogFile, ".log", ".files.js");
            var projectFiles = BatchBuildFiles.SaveProjectFiles(buildReportAssets, buildReportLog, tsvFiles, jsFiles);

            // Create final full build report results.
            var jsResult = SafeReplaceFileExtension(options.LogFile, ".log", ".build.result.js");
            BatchBuildResult.SaveBuildResult(buildReportAssets, buildReportLog, projectFiles, jsResult);

            // Create full HTML report.
            BuildReportAnalyzer.HtmlBuildReportFull();
            timer.Stop();
            Debug.Log($"batch_build_ exit time {timer.ElapsedTime}");
        }

        private static BuildReport BuildPLayer(BatchBuildOptions options)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(x => x.enabled)
                .Select(x => x.path)
                .ToArray();
            Assert.IsTrue(scenes.Length > 0, "Error: NO eligible SCENES FOUND for build in EditorBuildSettings");
            Debug.Log($"batch_build_ scenes {scenes.Length}: {string.Join(" ", scenes)}");
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = options.OutputPathName,
                options = options.BuildOptions,
                scenes = scenes,
                target = options.BuildTarget,
                targetGroup = options.BuildTargetGroup,
            };
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildPlayerOptions.targetGroup).Split(';');

            Debug.Log($"batch_build_ build productName: {Application.productName}");
            Debug.Log($"batch_build_ build version: {Application.version}");
            Debug.Log($"batch_build_ build bundleVersionCode: {PlayerSettings.Android.bundleVersionCode}");
            Debug.Log($"batch_build_ build output: {buildPlayerOptions.locationPathName}");

            // General settings we enforce for any build.
            PlayerSettings.insecureHttpOption = InsecureHttpOption.NotAllowed;
            Debug.Log($"batch_build_ insecureHttpOption: {PlayerSettings.insecureHttpOption}");

            switch (options.BuildTarget)
            {
                case BuildTarget.Android:
                {
                    // Android settings we enforce.
                    PlayerSettings.Android.minifyRelease = true;
                    PlayerSettings.Android.useCustomKeystore = true;
                    Debug.Log($"batch_build_ Android.minifyRelease: {PlayerSettings.Android.minifyRelease}");
                    Debug.Log($"batch_build_ Android.useCustomKeystore: {PlayerSettings.Android.useCustomKeystore}");
                    if (PlayerSettings.Android.useCustomKeystore)
                    {
                        // Project keystore:
                        PlayerSettings.Android.keystoreName = options.Android.keystoreName;
                        PlayerSettings.keystorePass = options.Android.keystorePassword;
                        // Project key:
                        PlayerSettings.Android.keyaliasName = options.Android.keyaliasName;
                        PlayerSettings.keyaliasPass = options.Android.aliasPassword;

                        Debug.Log($"batch_build_ Android.keystoreName: {PlayerSettings.Android.keystoreName} " +
                                  $"Exists={File.Exists(PlayerSettings.Android.keystoreName)}");
                        Debug.Log(
                            $"batch_build_ PlayerSettings.keystorePass: {FormatUtil.PasswordToLog(PlayerSettings.keystorePass)}");
                        Debug.Log($"batch_build_ Android.keyaliasName: {PlayerSettings.Android.keyaliasName}");
                        Debug.Log(
                            $"batch_build_ PlayerSettings.keyaliasPass: {FormatUtil.PasswordToLog(PlayerSettings.keyaliasPass)}");
                    }
                    break;
                }
                case BuildTarget.WebGL:
                    PlayerSettings.WebGL.compressionFormat = options.WebGL.compressionFormat;
                    Debug.Log($"batch_build_ WebGL.compressionFormat: {PlayerSettings.WebGL.compressionFormat}");
                    // No use to show stack trace in browser.
                    SetStackTraceLogType(StackTraceLogType.None);
                    break;
            }
            // This produces multi-line output!
            Debug.Log($"batch_build_ defines:\r\n{string.Join("\r\n", defines)}");

            if (Directory.Exists(options.OutputFolder))
            {
                Directory.Delete(options.OutputFolder, recursive: true);
            }
            Directory.CreateDirectory(options.OutputFolder);
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            // Reset StackTraceLogType after build.
            SetStackTraceLogType(StackTraceLogType.ScriptOnly);
            return buildReport;
        }

        private static void SetStackTraceLogType(StackTraceLogType logType)
        {
            Debug.Log($"batch_build_ SetStackTraceLogType: {logType}");
            PlayerSettings.SetStackTraceLogType(LogType.Error, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Log, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, logType);
        }

        private static bool VerifyUnityVersionForBuild(string unityVersion, out string editorVersion)
        {
            editorVersion = File
                .ReadAllLines(Path.Combine("ProjectSettings", "ProjectVersion.txt"))[0]
                .Split(" ")[1];
            return unityVersion == editorVersion;
        }

        private static string SafeReplaceFileExtension(string filename, string oldExtension, string newExtension)
        {
            Assert.IsTrue(oldExtension.StartsWith('.'));
            Assert.IsTrue(newExtension.StartsWith('.'));
            return filename.EndsWith(oldExtension)
                ? $"{filename.Substring(0, filename.Length - oldExtension.Length)}{newExtension}"
                : $"{filename}{newExtension}";
        }

        public static Dictionary<string, string> LoadSecretKeys(string path, BuildTarget buildTarget)
        {
            // This belong to BatchBuildOptions but it is exposed for for testing.
            var secretKeys = new Dictionary<string, string>();
            if (buildTarget == BuildTarget.Android)
            {
                var androidOptions = Path.Combine(path, $"{nameof(BatchBuildOptions.AndroidOptions)}.txt");
                if (!File.Exists(androidOptions))
                {
                    throw new UnityException($"batch_build_ file not found: {androidOptions}");
                }
                ParseSecretFile(androidOptions);
            }
#if USE_GA
            if (!Directory.Exists(path))
            {
                throw new UnityException($"batch_build_ directory not found: {path}");
            }
            var gameAnalyticsOptions = Path.Combine(path, $"{nameof(BatchBuildOptions.GameAnalyticsOptions)}.txt");
            if (!File.Exists(gameAnalyticsOptions))
            {
                throw new UnityException($"batch_build_ file not found: {gameAnalyticsOptions}");
            }
            ParseSecretFile(gameAnalyticsOptions);
#endif
            return secretKeys;

            void ParseSecretFile(string filename)
            {
                foreach (var line in File.ReadAllLines(filename)
                             .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")))
                {
                    var tokens = line.Split('=');
                    if (tokens.Length != 2 || tokens[0].Contains('#'))
                    {
                        throw new UnityException(
                            $"batch_build_ invalid line in file: {filename}, line: {line}");
                    }
                    if (!secretKeys.TryAdd(tokens[0].Trim(), tokens[1].Trim()))
                    {
                        throw new UnityException(
                            $"batch_build_ duplicate key in file: {filename}, line: {line}");
                    }
                }
            }
        }

        #region BatchBuildOptions

        private class BatchBuildOptions
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public class AndroidOptions
            {
                // PlayerSettings.Android.keyaliasName.
                public string keyaliasName;

                // This is path to android keystore file.
                public string keystoreName;

                // Two passwords required for the build as in PlayerSettings.
                public string keystorePassword;
                public string aliasPassword;
            }

            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public class WebGlOptions
            {
                // PlayerSettings.WebGL.compressionFormat
                // ReSharper disable once ConvertToConstant.Local
                public readonly WebGLCompressionFormat compressionFormat = WebGLCompressionFormat.Brotli;
            }

            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public class GameAnalyticsOptions
            {
                // Secret keys for current build platform that will be inject in GameAnalyticsSDK.Setup.Settings.
                public string gameKey;
                public string secretKey;
            }

            // Paths and file names.
            public readonly string ProjectPath;
            public readonly string LogFile;
            public readonly string EnvFile;
            public readonly bool IsRebuild;

            // Actual build settings etc.
            public readonly BuildTarget BuildTarget;
            public readonly BuildTargetGroup BuildTargetGroup;
            public readonly BuildOptions BuildOptions;
            public readonly string OutputPathName;
            public readonly AndroidOptions Android = new();
            public readonly WebGlOptions WebGL = new();
            public readonly GameAnalyticsOptions GameAnalytics = new();

            // Just for information, if needed.
            public readonly string OutputFolder;
            public readonly bool IsDevelopmentBuild;
            public readonly bool IsBuildReport;

            // Build post processing.
            public readonly string LogFilePost;

            public readonly bool IsTestRun;

            public BatchBuildOptions(string[] args)
            {
                // Parse command line arguments
                // -projectPath - project folder name (for UNITY)
                // -buildTarget - build target name (for UNITY)
                // -logFile - log file name (for UNITY)
                // -envFile - settings file name (for BatchBuild to read actual build options etc)
                // -rebuild - do not update build info
                {
                    var buildTargetName = string.Empty;
                    for (var i = 0; i < args.Length; ++i)
                    {
                        var arg = args[i];
                        switch (arg)
                        {
                            case "-projectPath":
                                i += 1;
                                ProjectPath = args[i];
                                if (!Directory.Exists(ProjectPath))
                                {
                                    throw new ArgumentException($"Directory -projectPath ${ProjectPath} is not found");
                                }
                                break;
                            case "-buildTarget":
                                i += 1;
                                buildTargetName = args[i];
                                break;
                            case "-logFile":
                                i += 1;
                                LogFile = args[i];
                                break;
                            case "-envFile":
                                i += 1;
                                EnvFile = args[i];
                                if (!File.Exists(EnvFile))
                                {
                                    throw new ArgumentException($"File -envFile '{EnvFile}' is not found");
                                }
                                break;
                            case "-rebuild":
                                IsRebuild = true;
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(ProjectPath))
                    {
                        throw new ArgumentException($"-projectPath is mandatory for batch build");
                    }
                    if (KnownBuildTargets.TryGetValue(buildTargetName, out var buildTarget))
                    {
                        BuildTarget = buildTarget.Item1;
                        BuildTargetGroup = buildTarget.Item2;
                        switch (BuildTarget)
                        {
                            // Primary.
                            case BuildTarget.Android:
                                break;
                            // Secondary.
                            case BuildTarget.WebGL:
                                break;
                            // For testing only.
                            case BuildTarget.StandaloneWindows64:
                                break;
                            default:
                                throw new UnityException($"Build target '{BuildTarget}' is not supported");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"-buildTarget '{buildTargetName}' is invalid or unsupported");
                    }
                    if (string.IsNullOrWhiteSpace(LogFile))
                    {
                        throw new ArgumentException($"-logFile is mandatory for batch build");
                    }
                    if (string.IsNullOrWhiteSpace(EnvFile))
                    {
                        throw new ArgumentException($"-envFile is mandatory for batch build");
                    }
                }

                // Parse settings file.
                var lines = File.ReadAllLines(EnvFile);
                var secretKeys = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var tokens = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 1 && line.Contains('='))
                    {
                        // Skip empty values!
                        continue;
                    }
                    var key = tokens[0].Trim();
                    var value = tokens[1].Trim();
                    switch (key)
                    {
                        case "IsTestRun":
                            IsTestRun = bool.Parse(value);
                            break;
                        case "IsDevelopmentBuild":
                            IsDevelopmentBuild = bool.Parse(value);
                            break;
                        case "IsBuildReport":
                            IsBuildReport = bool.Parse(value);
                            break;
                        case "SecretKeys":
                            if (!Directory.Exists(value))
                            {
                                throw new UnityException($"batch_build_ directory not found: {value}");
                            }
                            secretKeys = LoadSecretKeys(value, BuildTarget);
                            break;
                        case "LOG_FILE_POST":
                            // Variable is shared with commandline, thus it is in UPPER_CASE.
                            LogFilePost = value;
                            break;
                    }
                }
                // Create actual build options
                BuildOptions = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport;
                if (IsDevelopmentBuild)
                {
                    BuildOptions |= BuildOptions.Development;
                }
                // Set secret keys etc.
                if (BuildTarget == BuildTarget.Android)
                {
                    Android.keystoreName = secretKeys[nameof(Android.keystoreName)];
                    Android.keyaliasName = secretKeys[nameof(Android.keyaliasName)];
                    Android.keystorePassword = secretKeys[nameof(Android.keystorePassword)];
                    Android.aliasPassword = secretKeys[nameof(Android.aliasPassword)];
                }
#if USE_GA
                GameAnalytics.gameKey = secretKeys[$"{BuildTarget}_{nameof(GameAnalytics.gameKey)}"];
                GameAnalytics.secretKey = secretKeys[$"{BuildTarget}_{nameof(GameAnalytics.secretKey)}"];
#else
                Assert.IsNotNull(GameAnalytics);
#endif
                // Set final output path and name.
                OutputFolder = Path.Combine(ProjectPath, $"build{BuildPipeline.GetBuildTargetName(BuildTarget)}");
                if (BuildTarget == BuildTarget.WebGL)
                {
                    OutputPathName = OutputFolder;
                }
                else
                {
                    var appName =
                        SanitizePath(
                            $"{Application.productName}_{Application.version}_{PlayerSettings.Android.bundleVersionCode}");
                    var appExtension = BuildTarget == BuildTarget.Android ? "aab" : "exe";
                    OutputPathName = Path.Combine(OutputFolder, $"{appName}.{appExtension}");
                }
            }

            public override string ToString()
            {
                return
                    $"{nameof(ProjectPath)}: {ProjectPath}, {nameof(LogFile)}: {LogFile}, {nameof(EnvFile)}: {EnvFile}" +
                    $", {nameof(BuildTarget)}: {BuildTarget}, {nameof(BuildTargetGroup)}: {BuildTargetGroup}" +
                    $", {nameof(BuildOptions)}: [{BuildOptions}]" +
                    $", {nameof(OutputFolder)}: {OutputFolder}, {nameof(OutputPathName)}: {OutputPathName}" +
                    $", {nameof(IsDevelopmentBuild)}: {IsDevelopmentBuild}, {nameof(IsTestRun)}: {IsTestRun}" +
                    $", {nameof(LogFilePost)}: {LogFilePost}";
            }

            // Build target parameter mapping
            // See: https://docs.unity3d.com/Manual/CommandLineArguments.html
            // See: https://docs.unity3d.com/2019.4/Documentation/ScriptReference/BuildTarget.html
            // See: https://docs.unity3d.com/ScriptReference/BuildPipeline.GetBuildTargetName.html
            private static readonly Dictionary<string, Tuple<BuildTarget, BuildTargetGroup>> KnownBuildTargets = new()
            {
                {
                    /*" Win64" */ BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneWindows64),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.StandaloneWindows64,
                        BuildTargetGroup.Standalone)
                },
                {
                    /*" Android" */ BuildPipeline.GetBuildTargetName(BuildTarget.Android),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.Android, BuildTargetGroup.Android)
                },
                {
                    /*" WebGL" */ BuildPipeline.GetBuildTargetName(BuildTarget.WebGL),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.WebGL, BuildTargetGroup.WebGL)
                },
            };

            private static string SanitizePath(string path)
            {
                // https://www.mtu.edu/umc/services/websites/writing/characters-avoid/
                var illegalCharacters = new[]
                {
                    '#', '<', '$', '+',
                    '%', '>', '!', '`',
                    '&', '*', '\'', '|',
                    '{', '?', '"', '=',
                    '}', '/', ':', '@',
                    '\\', ' '
                };
                for (var i = 0; i < path.Length; ++i)
                {
                    var c = path[i];
                    if (illegalCharacters.Contains(c))
                    {
                        path = path.Replace(c, '_');
                    }
                }
                return path;
            }
        }

        #endregion
    }
}
