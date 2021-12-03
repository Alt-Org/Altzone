using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Conditional UnityEngine.Debug wrapper for development.
/// </summary>
public static class Debug
{
    // See: https://answers.unity.com/questions/126315/debuglog-in-build.html
    // StackFrame: https://stackoverflow.com/questions/21884142/difference-between-declaringtype-and-reflectedtype
    // Method: https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous

#if FORCE_LOG
#warning NOTE: Compiling WITH debug logging FORCE_LOG
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInitializeOnLoadMethod()
    {
        // Reset static fields even when Domain Reloading is disabled.
        _isClassNameColor = false;
        _classNameColor = null;
        _classNameColorFilter = null;
        CachedMethods.Clear();
        _logLineAllowedFilter = null;
    }

    private static string _classNameColorFilter;
    private static string _classNameColor;
    private static bool _isClassNameColor;

    // Cache methods if method lookup is expensive.
    private static readonly Dictionary<MethodBase, bool> CachedMethods = new Dictionary<MethodBase, bool>();

    /// <summary>
    /// Filters log lines based on method name or other method properties.
    /// </summary>
    private static Func<MethodBase, bool> _logLineAllowedFilter;

    /// <summary>
    /// Adds log line filter.
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void AddLogLineAllowedFilter(Func<MethodBase, bool> filter)
    {
        _logLineAllowedFilter += filter;
    }

    /// <summary>
    /// Sets color for class name field in debug log line.
    /// </summary>
    /// <remarks>
    /// See: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
    /// and
    /// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
    /// </remarks>
    /// <param name="colorName">Unity color name</param>
    /// <param name="logLineContentFilter">log writer filter</param>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void SetColorForClassName(string colorName, ref Func<string, string> logLineContentFilter)
    {
        string RemoveColorFromLogLine(string line)
        {
            // getPrefix will add color to be removed here:
            // [<color={classNameColor}>{className}</color>]
            return line.Replace(_classNameColorFilter, "[").Replace("</color>]", "]");
        }

        if (string.IsNullOrWhiteSpace(colorName))
        {
            _isClassNameColor = false;
            _classNameColor = null;
            _classNameColorFilter = null;
            logLineContentFilter -= RemoveColorFromLogLine;
        }
        else
        {
            _isClassNameColor = true;
            _classNameColor = colorName;
            _classNameColorFilter = $"[<color={_classNameColor}>";
            logLineContentFilter += RemoveColorFromLogLine;
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void Log(string message)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.Log(message);
        }
        else if (IsMethodAllowedForLog(method))
        {
            UnityEngine.Debug.Log($"{GetPrefix(method)}{message}");
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogFormat(string format, params object[] args)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
        else if (IsMethodAllowedForLog(method))
        {
            UnityEngine.Debug.LogFormat($"{GetPrefix(method)}{format}", args);
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogWarning(string message, Object context = null)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }

    public static void LogError(string message, Object context = null)
    {
        UnityEngine.Debug.LogError(message, context);
    }

    public static void LogException(Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }

    private static string GetPrefix(MemberInfo method)
    {
        var className = method.ReflectedType?.Name ?? nameof(Debug);
        if (className.StartsWith("<"))
        {
            // For anonymous types we try its parent type.
            className = method.ReflectedType?.DeclaringType?.Name ?? nameof(Debug);
        }
        // removeColorFromLogLine will remove this if logged to file
        return _isClassNameColor
            ? $"[<color={_classNameColor}>{className}</color>] "
            : $"[{className}] ";
    }

    private static bool IsMethodAllowedForLog(MethodBase method)
    {
        if (_logLineAllowedFilter != null)
        {
            if (CachedMethods.TryGetValue(method, out var isMethodAllowed))
            {
                return isMethodAllowed;
            }
            // Invocation list works like OR and it will use short-circuit evaluation.
            var invocationList = _logLineAllowedFilter.GetInvocationList();
            foreach (var callback in invocationList)
            {
                var result = callback.DynamicInvoke(method);
                if (result is bool isAllowed && isAllowed)
                {
                    CachedMethods.Add(method, true);
                    //UnityEngine.Debug.Log($"[<color=brown>ACCEPT</color>] {method.Name} in {method.ReflectedType?.FullName}");
                    return true;
                }
            }
            // Nobody accepted so it is rejected.
            CachedMethods.Add(method, false);
            //UnityEngine.Debug.Log($"[<color=brown>REJECT</color>] {method.Name} in {method.ReflectedType?.FullName}");
            return false;
        }
        return true;
    }
}