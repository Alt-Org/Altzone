using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Debug logger config for test and production.
    /// </summary>
    /// <remarks>
    /// Uncomment CreateAssetMenu line below to create <c>LoggerConfig</c> asset for new project using Assets->Create menu<br />
    /// and save it in version control and then comment CreateAssetMenu line again.<br />
    /// Comment or delete CreateAssetMenu line for LocalLoggerConfig!
    /// </remarks>
    // [CreateAssetMenu(menuName = "ALT-Zone/" + nameof(LoggerConfig))]
    [CreateAssetMenu(menuName = "ALT-Zone/LocalLoggerConfig", fileName = "LocalLoggerConfig")]
    public class LoggerConfig : ScriptableObject
    {
        /// <summary>
        /// Immutable <c>RegExFilter</c> contains regex pattern and flag to include (log) or exclude (skip) given class from logging.<br />
        /// See https://regexr.com/
        /// </summary>
        private class RegExFilter
        {
            public readonly Regex Regex;
            public readonly bool IsLogged;

            public RegExFilter(string regex, bool isLogged)
            {
                const RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;
                Regex = new Regex(regex, regexOptions);
                IsLogged = isLogged;
            }
        }

        private const string TooltipLog = "Is logging to file enabled";
        private const string TooltipColor = "Color for logged classname";
        private const string TooltipContext = "Color to 'mark' logged context objects";
        private const string TooltipRegExp = "Regular expressions with 1/0 to match logged lines and enable/disable their logging";

        [Header("Settings"), Tooltip(TooltipLog)] public bool _isLogToFile;
        [Tooltip(TooltipColor)] public string _colorForClassName = "white";
        [Tooltip(TooltipContext)] public string _colorForContextTagName = "orange";

        [Header("Class Filter"), TextArea(5, 20), Tooltip(TooltipRegExp)] public string _loggerRules;

        private static string _prefixTag;
        private static string _suffixTag;
        private static readonly HashSet<string> LoggedTypesForEditor = new();

        /// <summary>
        /// Creates a config for filtering (out) some Debug logging messages.
        /// </summary>
        /// <param name="config">the config</param>
        /// <param name="setLoggedDebugTypes">callback to record all types that are suing Debug.log calls in Editor</param>
        public static void CreateLoggerFilterConfig(LoggerConfig config, Action<string> setLoggedDebugTypes)
        {
            string FilterClassNameForLogMessageCallback(string message)
            {
                return message.Replace(_prefixTag, "[").Replace(_suffixTag, "]");
            }

            if (config._isLogToFile)
            {
                CreateLogWriter();
            }
            if (AppPlatform.IsEditor)
            {
                // Log color for Editor log.
                // https://docs.unity3d.com/560/Documentation/Manual/StyledText.html
                var trimmed = string.IsNullOrEmpty(config._colorForClassName) ? string.Empty : config._colorForClassName.Trim();
                if (trimmed.Length > 0)
                {
                    // This is a bit complicated because:
                    // - Debug knows what to log and adds color to logged content that goes to UnityEngine console logger
                    // - LogWriter receives it and needs to remove those parts that are only for console logging, like colors.
                    _prefixTag = $"[<color={config._colorForClassName}>";
                    _suffixTag = "</color>]";
                    Debug.SetTagsForClassName(_prefixTag, _suffixTag);
                    LogWriter.AddLogLineContentFilter(FilterClassNameForLogMessageCallback);
                }
                if (!string.IsNullOrWhiteSpace(config._colorForContextTagName))
                {
                    Debug.SetContextTag($"<color={config._colorForContextTagName}>*</color>");
                }
                // Clear previous run.
                LoggedTypesForEditor.Clear();
                setLoggedDebugTypes?.Invoke(string.Empty);
            }
            var capturedRegExFilters = BuildFilter(config._loggerRules ?? string.Empty);
            if (capturedRegExFilters.Count == 0)
            {
#if UNITY_EDITOR
                // Add greedy filter to catch everything in Editor.
                capturedRegExFilters.Add(new RegExFilter("^.*=1", true));
#else
                // No filtering configured, everything will be logged by default.
                return;
#endif
            }

            // Install log filter callback as last thing here.
            Debug.AddLogLineAllowedFilter(LogLineAllowedFilterCallback);
#if FORCE_LOG || UNITY_EDITOR
#else
            UnityEngine.Debug.LogWarning($"NOTE! Application logging is totally disabled on platform: {Application.platform}");
#endif

            bool LogLineAllowedFilterCallback(MethodBase method)
            {
                // For anonymous types we try its parent type.
                var isAnonymous = method.ReflectedType?.Name.StartsWith("<");
                var type = isAnonymous.HasValue && isAnonymous.Value
                    ? method.ReflectedType?.DeclaringType
                    : method.ReflectedType;
                if (type?.FullName == null)
                {
                    // Should not happen in this context because a method should have a class (even anonymous).
                    return true;
                }
#if UNITY_EDITOR
                // Collect all logged types in Editor just for fun.
                if (LoggedTypesForEditor.Add(type.FullName))
                {
                    var list = LoggedTypesForEditor.ToList();
                    list.Sort();
                    // Lines should be put into something/somewhere that is not kept in the version control if it is saved on the disk.
                    setLoggedDebugTypes?.Invoke(string.Join('\n', list));
                }
#endif
                // If filter does not match we log them always.
                // - add filter rule "^.*=0" to disable everything after this point
                var match = capturedRegExFilters.FirstOrDefault(x => x.Regex.IsMatch(type.FullName));
                return match?.IsLogged ?? true;
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
        private static void CreateLogWriter()
        {
            string FilterPhotonLogMessage(string message)
            {
                // This is mainly to remove "formatting" form Photon ToString and ToStringFull messages and make then one liners!
                if (!string.IsNullOrEmpty(message))
                {
                    if (message.Contains("\n") || message.Contains("\r") || message.Contains("\t"))
                    {
                        message = message.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                    }
                }
                return message;
            }

            UnitySingleton.CreateStaticSingleton<LogWriter>();
            LogWriter.AddLogLineContentFilter(FilterPhotonLogMessage);
        }

        private static List<RegExFilter> BuildFilter(string lines)
        {
            // Note that line parsing relies on TextArea JSON serialization which I have not tested very well!
            // - lines can start and end with "'" if content has something that needs to be "protected" during JSON parsing
            // - JSON multiline separator is LF "\n"
            var list = new List<RegExFilter>();
            if (lines.StartsWith("'") && lines.EndsWith("'"))
            {
                lines = lines.Substring(1, lines.Length - 2);
            }
            foreach (var token in lines.Split('\n'))
            {
                var line = token.Trim();
                if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                {
                    continue;
                }
                try
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2)
                    {
                        UnityEngine.Debug.LogError($"invalid Regex pattern '{line}', are you missing '=' here");
                        continue;
                    }
                    if (!int.TryParse(parts[1].Trim(), out var loggedValue))
                    {
                        UnityEngine.Debug.LogError($"invalid Regex pattern '{line}', not a valid integer after '=' sign");
                        continue;
                    }
                    var isLogged = loggedValue != 0;
                    var filter = new RegExFilter(parts[0].Trim(), isLogged);
                    list.Add(filter);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"invalid Regex pattern '{line}': {e.GetType().Name} {e.Message}");
                }
            }
            return list;
        }
    }
}