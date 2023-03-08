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
    /// Debug logger config.
    /// </summary>
    public class LoggerConfig : ScriptableObject
    {
        /// <summary>
        /// Class <c>RegExpFilter</c> contains regexp pattern and flag to include or exclude class name filter patterns from logging.
        /// </summary>
        private class RegExpFilter
        {
            public bool IsLogged;
            public Regex Regex;
        }

        private const string Tooltip1 = "Default value for non matching debug lines";
        private const string Tooltip2 = "Is logging files without a namespace forced to be logged always";
        private const string Tooltip3 = "Is logging to file enabled";
        private const string Tooltip4 = "Color for logged classname";
        private const string Tooltip5 = "Color to 'mark' logged context objects";
        private const string Tooltip6 = "Regular expressions with 1/0 to match logged lines and enable/disable their logging";

        [Header("Settings"), Tooltip(Tooltip1)] public bool _isDefaultMatchTrue;
        [Tooltip(Tooltip2)] public bool _isLogNoNamespaceForced;
        [Tooltip(Tooltip3)] public bool _isLogToFile;
        [Tooltip(Tooltip4)] public string _colorForClassName = "white";
        [Tooltip(Tooltip5)] public string _colorForContextTagName = "orange";

        [Header("Class Filter"), TextArea(5, 20), Tooltip(Tooltip6)] public string _loggerRules;

        [Header("Classes Seen"), TextArea(5, 20), Tooltip(Tooltip6)] public string _loggedTypes;

        private static string _prefixTag;
        private static string _suffixTag;
        private static readonly HashSet<string> LoggedTypesForEditor = new();

        public static void CreateLoggerConfig(LoggerConfig config)
        {
            string FilterClassNameLogMessage(string message)
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
                    LogWriter.AddLogLineContentFilter(FilterClassNameLogMessage);
                }
                if (!string.IsNullOrWhiteSpace(config._colorForContextTagName))
                {
                    Debug.SetContextTag($"<color={config._colorForContextTagName}>*</color>");
                }
                // Clear previous run.
                LoggedTypesForEditor.Clear();
                config._loggedTypes = string.Empty;
            }
            var filterList = config.BuildFilter();
            if (filterList.Count == 0)
            {
                return;
            }

            // Install log filter as last thing here.
            bool LogLineAllowedFilter(MethodBase method)
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
                if (LoggedTypesForEditor.Add(type.FullName))
                {
                    var list = LoggedTypesForEditor.ToList();
                    list.Sort();
                    config._loggedTypes = string.Join('\n', list);
                }
#endif
                if (config._isLogNoNamespaceForced && string.IsNullOrEmpty(type.Namespace))
                {
                    // Should not have classes without a namespace but allow logging for them anyways.
                    return true;
                }
                var match = filterList.FirstOrDefault(x => x.Regex.IsMatch(type.FullName));
                return match?.IsLogged ?? config._isDefaultMatchTrue;
            }

            Debug.AddLogLineAllowedFilter(LogLineAllowedFilter);
#if FORCE_LOG || UNITY_EDITOR
#else
            UnityEngine.Debug.LogWarning($"NOTE! Application logging is totally disabled on platform: {Application.platform}");
#endif
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

        private List<RegExpFilter> BuildFilter()
        {
            // Note that line parsing relies on TextArea JSON serialization which I have not tested very well!
            // - lines can start and end with "'" if content has something that needs to be "protected" during JSON parsing
            // - JSON multiline separator is LF "\n"
            var list = new List<RegExpFilter>();
            var lines = _loggerRules ?? string.Empty;
            if (lines.StartsWith("'") && lines.EndsWith("'"))
            {
                lines = lines.Substring(1, lines.Length - 2);
            }
            foreach (var token in lines.Split('\n'))
            {
                var line = token.Trim();
                if (line.StartsWith("#"))
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
                    const RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;
                    var filter = new RegExpFilter
                    {
                        Regex = new Regex(parts[0].Trim(), regexOptions),
                        IsLogged = isLogged
                    };
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