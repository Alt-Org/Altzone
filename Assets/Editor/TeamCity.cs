using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Utility class to perform command line builds.<br />
    /// See <c>CommandLine</c> for supported command line options.
    /// </summary>
    /// <remarks>
    /// Should be compatible with CI systems.<br />
    /// For example TeamCity, Jenkins and CircleCI are some well known CI/CD systems.
    /// </remarks>
    internal static class TeamCity
    {
        private const string LogPrefix = "LOG_" + nameof(TeamCity);
        private const string LogSeparator = "========================================";

        private const string OutputFolderAndroid = "buildAndroid";
        private const string OutputFolderWebgl = "buildWebGL";
        private const string OutputFolderWin64 = "buildWin64";

        private static readonly List<string> LogMessages = new List<string>
        {
            $"{LogPrefix} {LogSeparator}",
        };

        private static string OutputBaseFilename =>
            SanitizePath($"{Application.productName}_{Application.version}_{PlayerSettings.Android.bundleVersionCode}");

        private static string[] Scenes => EditorBuildSettings.scenes
            .Where(x => x.enabled)
            .Select(x => x.path)
            .ToArray();

        internal static void CheckAndroidBuild()
        {
            // We assume that local keystore and password folder is one level up from current working directory
            // - that should be UNITY project folder
            var keystore = Path.Combine("..", $"local_{GetCurrentUser()}", "altzone.keystore");
            var args = CommandLine.Parse(new[] { "-buildTarget", "Android", "-keystore", keystore });
            configure_Android(args);
            Log($"output filename: {GetOutputFile(args.BuildTarget)}");
        }

        //[MenuItem("Window/ALT-Zone/Build/Test/Android Build Post Processing")]
        private static void do_Android_Build_Post_processing()
        {
            const string scriptName = "m_BuildScript_PostProcess.bat";
            var symbolsName = $"{OutputBaseFilename}-{Application.version}-v{PlayerSettings.Android.bundleVersionCode}.symbols";
            var script = MyCmdLineScripts.AndroidPostProcessScript.Replace("<<altzone_symbols_name>>", symbolsName);
            File.WriteAllText(scriptName, script);
            Debug.Log($"PostProcess script '{scriptName}' written");
        }

        //[MenuItem("Window/ALT-Zone/Build/Test/WebGL Build Post Processing")]
        private static void do_WebGL_Build_Post_processing()
        {
            void PatchIndexHtml(string htmlFile, string curTitle, string newTitle)
            {
                var htmlContent = File.ReadAllText(htmlFile);
                var oldTitleText = $"<div class=\"title\">{curTitle}</div>";
                var newTitleText = $"<div class=\"title\">{newTitle}</div>";
                var newHtmlContent = htmlContent.Replace(oldTitleText, newTitleText);
                if (newHtmlContent == htmlContent)
                {
                    Log($"COULD NOT update file {htmlFile}, old title should be '{oldTitleText}'");
                    return;
                }
                Log($"update file {htmlFile}");
                Log($"old html title '{oldTitleText}'");
                Log($"new html title '{newTitleText}'");
                File.WriteAllText(htmlFile, newHtmlContent);
            }

            var indexHtml = Path.Combine(OutputFolderWebgl, "index.html");
            var curName = Application.productName;
            var title = $"{Application.productName} built {DateTime.Now:u}";
            var gitTagCompliantLabel =
                title.Substring(0, title.Length - 4) // remove seconds
                    .Replace(" ", "_")
                    .Replace(":", ".");
            PatchIndexHtml(indexHtml, curName, gitTagCompliantLabel);

            const string scriptName = "m_BuildScript_PostProcess.bat";
            File.WriteAllText(scriptName, MyCmdLineScripts.WebGLPostProcessScript);
            Debug.Log($"PostProcess script '{scriptName}' written");
        }

        internal static void CreateBuildScript()
        {
            const string scriptName = "m_BuildScript.bat";
            var sep1 = Path.AltDirectorySeparatorChar.ToString();
            var sep2 = Path.DirectorySeparatorChar.ToString();
            var unityName = EditorApplication.applicationPath.Replace(sep1, sep2);
            var methodName = $"{typeof(TeamCity).FullName}.{nameof(Build)}";
            var script = MyCmdLineScripts.BuildScript
                .Replace("<<unity_name>>", unityName)
                .Replace("<<method_name>>", methodName);
            File.WriteAllText(scriptName, script);
            Debug.Log($"Build script '{scriptName}' written");
            var buildTargetName = CommandLine.BuildTargetNameFrom(EditorUserBuildSettings.activeBuildTarget);
            var driverName = $"{Path.GetFileNameWithoutExtension(scriptName)}_{buildTargetName}.bat";
            var driverScript = $"{scriptName} {buildTargetName} && pause";
            File.WriteAllText(driverName, driverScript);
            Debug.Log($"Build script driver '{driverName}' written");
        }

        internal static void Build()
        {
            BuildResult buildResult;
            try
            {
                DumpEnvironment();
                var args = CommandLine.Parse(Environment.GetCommandLineArgs());
                Log($"build with args: {args}");
                var buildOptions = BuildOptions.None;
                if (args.IsDevelopmentBuild)
                {
                    buildOptions |= BuildOptions.Development;
                }
                string outputDir;
                BuildTargetGroup targetGroup;
                switch (args.BuildTarget)
                {
                    case BuildTarget.Android:
                        outputDir = Path.Combine(OutputFolderAndroid, GetOutputFile(args.BuildTarget));
                        targetGroup = BuildTargetGroup.Android;
                        configure_Android(args);
                        break;
                    case BuildTarget.WebGL:
                        outputDir = OutputFolderWebgl;
                        targetGroup = BuildTargetGroup.WebGL;
                        break;
                    case BuildTarget.StandaloneWindows64:
                        outputDir = Path.Combine(OutputFolderWin64, GetOutputFile(args.BuildTarget));
                        targetGroup = BuildTargetGroup.Standalone;
                        break;
                    default:
                        throw new UnityException($"build target '{args.BuildTarget}' not supported");
                }
                // Output (artifacts) should be inside project folder for CI systems to find them
                var buildPlayerOptions = new BuildPlayerOptions
                {
                    locationPathName = Path.Combine(args.ProjectPath, outputDir),
                    options = buildOptions,
                    scenes = Scenes,
                    target = args.BuildTarget,
                    targetGroup = targetGroup,
                };

                Log($"build productName: {Application.productName}");
                Log($"build version: {Application.version}");
                Log($"build bundleVersionCode: {PlayerSettings.Android.bundleVersionCode}");
                Log($"build output: {buildPlayerOptions.locationPathName}");
                if (Directory.Exists(buildPlayerOptions.locationPathName))
                {
                    Directory.Delete(buildPlayerOptions.locationPathName, recursive: true);
                }
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';');
                Log($"build defines:\r\n{string.Join("\r\n", defines)}");
                var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
                var summary = buildReport.summary;
                buildResult = summary.result;
                Log($"build result: {buildResult}");
                if (buildResult == BuildResult.Succeeded)
                {
                    if (summary.platform == BuildTarget.Android)
                    {
                        do_Android_Build_Post_processing();
                    }
                    else if (summary.platform == BuildTarget.WebGL)
                    {
                        do_WebGL_Build_Post_processing();
                    }
                }
            }
            catch (Exception x)
            {
                Log($"Unhandled exception: {x.Message} ({x.GetType().FullName})");
                throw;
            }
            finally
            {
                if (LogMessages.Count > 0)
                {
                    // Show all logged messages together without call stack for convenience!
                    LogMessages.Add($"{LogPrefix} {LogSeparator}");
                    Debug.Log($"{LogPrefix} LOG_MESSAGES:\r\n{string.Join("\r\n", LogMessages)}");
                }
            }
            // We must exit outside try-finally block as it seems that EditorApplication.Exit does not allow C# to unwind call stack properly
            if (buildResult != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        private static string GetOutputFile(BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.WebGL)
            {
                return "buildWebGL";
            }
            string extension;
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    extension = "aab";
                    break;
                case BuildTarget.StandaloneWindows64:
                    extension = "exe";
                    break;
                default:
                    throw new UnityException($"getOutputFile: build target '{buildTarget}' not supported");
            }
            var filename = $"{OutputBaseFilename}.{extension}";
            return filename;
        }

        private static void configure_Android(CommandLine args)
        {
            string GetLocalPasswordFor(string folder, string filename)
            {
                var file = Path.Combine(folder, filename);
                if (File.Exists(file))
                {
                    return File.ReadAllLines(file)[0];
                }
                throw new UnityException($"getLocalPasswordFor: file '{file}' not found");
            }

            void LogObfuscated(string name, string value)
            {
                var result = (value == null || value.Length < 9)
                    ? "******"
                    : value.Substring(0, 3) + "******" + value.Substring(value.Length - 3);
                Log($"{name}={result}");
            }

            // Enable application signing with a custom keystore!
            // - Android.keystoreName : as command line parameter
            // - keystorePass : read from keystore folder
            // - Android.keyaliasName : product name in lowercase
            // - keyaliasPass : read from keystore folder

            Log("configure_Android");
            PlayerSettings.Android.keystoreName = args.KeystoreName;
            Log($"keystoreName={PlayerSettings.Android.keystoreName}");

            // EditorUserBuildSettings
            EditorUserBuildSettings.buildAppBundle = true; // For Google Play this must be always true!
            Log($"buildAppBundle={EditorUserBuildSettings.buildAppBundle}");
            if (args.IsAndroidFull)
            {
                EditorUserBuildSettings.androidCreateSymbolsZip = true;
                EditorUserBuildSettings.androidReleaseMinification = AndroidMinification.Proguard;
            }
            else
            {
                // Do not change current settings!
            }
            Log($"androidCreateSymbolsZip={EditorUserBuildSettings.androidCreateSymbolsZip}");
            Log($"androidReleaseMinification={EditorUserBuildSettings.androidReleaseMinification}");

            PlayerSettings.Android.useCustomKeystore = true;
            Log($"useCustomKeystore={PlayerSettings.Android.useCustomKeystore}");
            PlayerSettings.Android.keyaliasName = Application.productName.ToLower();
            Log($"keyaliasName={PlayerSettings.Android.keyaliasName}");

            if (!File.Exists(PlayerSettings.Android.keystoreName))
            {
                throw new UnityException($"Keystore file '{PlayerSettings.Android.keystoreName}' not found, can not sign without it");
            }

            // Password files must be in same folder where keystore is!
            var passwordFolder = Path.GetDirectoryName(args.KeystoreName);
            Log($"passwordFolder={passwordFolder}");
            PlayerSettings.keystorePass = GetLocalPasswordFor(passwordFolder, "keystore_password");
            LogObfuscated("keystorePass", PlayerSettings.keystorePass);
            PlayerSettings.keyaliasPass = GetLocalPasswordFor(passwordFolder, "alias_password");
            LogObfuscated("keyaliasPass", PlayerSettings.keyaliasPass);
        }

        private static string SanitizePath(string path)
        {
            // https://www.mtu.edu/umc/services/websites/writing/characters-avoid/
            var illegalCharacters = new[]
            {
                '#', '<', '$', '+',
                '%', '>', '!', '`',
                '&', '*', '\'', '|',
                '{', '?', '"', '=',
                '}', '/', ':',
                '\\', ' ', '@',
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

        private static string GetCurrentUser()
        {
            var variables = Environment.GetEnvironmentVariables();
            foreach (var key in variables.Keys)
            {
                if (key.Equals("USERNAME"))
                {
                    return variables[key].ToString();
                }
            }
            throw new ArgumentException("Environment variable 'USERNAME' not found");
        }

        private static void DumpEnvironment()
        {
            var variables = Environment.GetEnvironmentVariables();
            var keys = variables.Keys.Cast<string>().ToList();
            keys.Sort();
            var builder = new StringBuilder($"GetEnvironmentVariables: {variables.Count}");
            foreach (var key in keys)
            {
                var value = variables[key];
                builder.AppendLine().Append($"{key}={value}");
            }
            Log(builder.ToString());
        }

        private static void Log(string message)
        {
            Debug.Log($"{LogPrefix} {message}");
            LogMessages.Add(message);
        }

        /// <summary>
        /// CommandLine class to parse and hold UNITY standard command line parameters and some custom build parameters.
        /// </summary>
        public class CommandLine
        {
            // Standard UNITY command line parameters.
            public readonly string ProjectPath;
            public readonly BuildTarget BuildTarget;

            // Custom build parameters.
            public readonly string KeystoreName;
            public readonly bool IsDevelopmentBuild;
            public readonly bool IsAndroidFull;

            private CommandLine(string projectPath, BuildTarget buildTarget, string keystoreName, bool isDevelopmentBuild, bool isAndroidFull)
            {
                this.ProjectPath = projectPath;
                this.BuildTarget = buildTarget;
                this.KeystoreName = keystoreName;
                this.IsDevelopmentBuild = isDevelopmentBuild;
                this.IsAndroidFull = isAndroidFull;
            }

            public override string ToString()
            {
                return
                    $"{nameof(ProjectPath)}: {ProjectPath}, {nameof(BuildTarget)}: {BuildTarget}, {nameof(KeystoreName)}: {KeystoreName}" +
                    $", {nameof(IsDevelopmentBuild)}: {IsDevelopmentBuild}, {nameof(IsAndroidFull)}: {IsAndroidFull}";
            }

            // Build target parameter mapping
            // See: https://docs.unity3d.com/Manual/CommandLineArguments.html
            // See: https://docs.unity3d.com/2019.4/Documentation/ScriptReference/BuildTarget.html
            private static readonly Dictionary<string, BuildTarget> KnownBuildTargets = new Dictionary<string, BuildTarget>
            {
                { "Win64", BuildTarget.StandaloneWindows64 },
                { "Android", BuildTarget.Android },
                { "WebGL", BuildTarget.WebGL },
            };

            public static string BuildTargetNameFrom(BuildTarget buildTarget)
            {
                var pair = KnownBuildTargets.FirstOrDefault(x => x.Value == buildTarget);
                return !string.IsNullOrEmpty(pair.Key) ? pair.Key : "Unknown";
            }

            public static CommandLine Parse(string[] args)
            {
                var projectPath = "./";
                var buildTarget = BuildTarget.StandaloneWindows64;
                var keystore = string.Empty;
                var isDevelopmentBuild = false;
                var isAndroidFull = false;
                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "-projectPath":
                            i += 1;
                            projectPath = args[i];
                            break;
                        case "-buildTarget":
                            i += 1;
                            if (!KnownBuildTargets.TryGetValue(args[i], out buildTarget))
                            {
                                throw new ArgumentException($"BuildTarget '{args[i]}' is invalid or unsupported");
                            }
                            break;
                        case "-keystore":
                            i += 1;
                            keystore = args[i];
                            break;
                        case "-DevelopmentBuild":
                            isDevelopmentBuild = true;
                            break;
                        case "-AndroidFull":
                            isAndroidFull = true;
                            break;
                    }
                }
                return new CommandLine(projectPath, buildTarget, keystore, isDevelopmentBuild, isAndroidFull);
            }
        }

        /// <summary>
        /// Collection of command line scripts our build "system".
        /// </summary>
        private static class MyCmdLineScripts
        {
            public static string BuildScript => BuildScriptContent;
            public static string AndroidPostProcessScript => AndroidPostProcessScriptContent;
            public static string WebGLPostProcessScript => WebGLPostProcessScriptContent;

            private const string BuildScriptContent = @"@echo off
set UNITY=<<unity_name>>

set BUILDTARGET=%1
if ""%BUILDTARGET%"" == ""Win64"" goto :valid_build
if ""%BUILDTARGET%"" == ""Android"" goto :valid_build
if ""%BUILDTARGET%"" == ""WebGL"" goto :valid_build
echo *
echo * Can not build: invalid build target '%BUILDTARGET%'
echo *
echo * Build target must be one of UNITY command line build target:
echo *
echo *	Win64
echo *	Android
echo *	WebGL
echo *
goto :eof

:valid_build

set PROJECTPATH=./
set METHOD=<<method_name>>
set LOGFILE=m_Build_%BUILDTARGET%.log
if ""%BUILDTARGET%"" == ""Android"" (
    set ANDROID_KEYSTORE=-keystore ..\local_%USERNAME%\altzone.keystore
)
rem try to simulate TeamCity invocation
set CUSTOM_OPTIONS=%ANDROID_KEYSTORE%
set UNITY_OPTIONS=-batchmode -projectPath %PROJECTPATH% -buildTarget %BUILDTARGET% -executeMethod %METHOD% %CUSTOM_OPTIONS% -quit -logFile ""%LOGFILE%""

set build_output=build%BUILDTARGET%
if exist %build_output% (
    echo Delete folder %build_output%
    rmdir /S /Q %build_output%
)
echo Start build
echo ""%UNITY%"" %UNITY_OPTIONS%
""%UNITY%"" %UNITY_OPTIONS%
set RESULT=%ERRORLEVEL%
if not ""%RESULT%"" == ""0"" (
    echo *
    echo * Build FAILED with %RESULT%, check log for errors
    echo *
    goto :eof
)
if not exist m_BuildScript_PostProcess.bat (
    echo Build done, check log for results
    goto :eof
)
echo Build done, start post processing
echo *
call m_BuildScript_PostProcess.bat
echo *
echo Post processing done
";

            private const string AndroidPostProcessScriptContent = @"@echo off
set BUILD_DIR=BuildAndroid
set DROPBOX_DIR=C:\Users\%USERNAME%\Dropbox\tekstit\altgame\BuildAndroid
set ZIP=C:\Program Files\7-Zip\7z.exe

echo BUILD_DIR=%BUILD_DIR%
echo DROPBOX_DIR=%DROPBOX_DIR%
echo ZIP=%ZIP%

if not exist ""%BUILD_DIR%"" (
    goto :eof
)

if not exist ""%ZIP%"" (
    echo ZIP not found
    goto :dropbox
)
:zip_symbols
set SYMBOLS_STORED=%BUILD_DIR%\<<altzone_symbols_name>>.zip
set SYMBOLS_DEFLATED=%BUILD_DIR%\<<altzone_symbols_name>>.deflated.zip
if not exist ""%SYMBOLS_STORED%"" (
    echo No symbols.zip file found
    goto :dropbox
)

set TEMP_SYMBOLS=%BUILD_DIR%\temp_symbols
echo UNZIP symbols to %TEMP_SYMBOLS%
if exist ""%TEMP_SYMBOLS%"" rmdir /S /Q ""%TEMP_SYMBOLS%""
""%ZIP%"" x -y -o""%TEMP_SYMBOLS%"" ""%SYMBOLS_STORED%""
set RESULT=%ERRORLEVEL%
echo UNZIP result %RESULT%
if not ""%RESULT%"" == ""0"" (
    echo UNZIP symbols failed
    exit /B 1
)

echo ZIP deflate symbols
if exist %SYMBOLS_DEFLATED% del /Q %SYMBOLS_DEFLATED%
""%ZIP%"" a -y -bd ""%SYMBOLS_DEFLATED%"" "".\%TEMP_SYMBOLS%\*""
set RESULT=%ERRORLEVEL%
echo ZIP result %RESULT%
if not ""%RESULT%"" == ""0"" (
    echo ZIP deflate symbols failed
    exit /B 1
)
echo clean up temp dir
if exist ""%SYMBOLS_STORED%"" del /Q ""%SYMBOLS_STORED%""
if exist ""%TEMP_SYMBOLS%"" rmdir /S /Q ""%TEMP_SYMBOLS%""
goto :dropbox

:dropbox
if not exist ""%DROPBOX_DIR%"" (
    goto :eof
)
if ""%LOGFILE%""  == """" (
    set LOGFILE=%0.log
)
robocopy ""%BUILD_DIR%"" ""%DROPBOX_DIR%"" /S /E /V /NP /R:0 /W:0 /LOG+:%LOGFILE%
set RESULT=%ERRORLEVEL%
echo ROBOCOPY result %RESULT%
goto :eof
";

            private const string WebGLPostProcessScriptContent = @"@echo off
set BUILD_DIR=BuildWebGL
set DROPBOX_DIR=C:\Users\%USERNAME%\Dropbox\tekstit\altgame\BuildWebGL
echo BUILD_DIR=%BUILD_DIR%
echo DROPBOX_DIR=%DROPBOX_DIR%
if not exist %DROPBOX_DIR% (
    goto :eof
)
if ""%LOGFILE%""  == """" (
    set LOGFILE=%0.log
)
robocopy %BUILD_DIR% %DROPBOX_DIR% /S /E /V /NP /R:0 /W:0 /LOG+:%LOGFILE%
goto :eof
";
        }
    }
}