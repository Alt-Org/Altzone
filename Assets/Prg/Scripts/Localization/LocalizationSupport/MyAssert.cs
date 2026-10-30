using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Prg
{
    /// <summary>
    /// This class is replacement for <c>UnityEngine.Assertions.Assert</c> and effective when <b>PRG_ASSERT</b> is defined.
    /// </summary>
    public static class MyAssert
    {
        private const string ErrorPrefix = "Assertion failed";
        private const string DefaultError = "no message";

        private static Object AsContext(object instance) => instance as Object;

        [Conditional("PRG_ASSERT")]
        public static void AreEqual<T>(T expected, T actual, string message, object context, [CallerMemberName] string memberName = null)
        {
            var areEqual = EqualityComparer<T>.Default.Equals(expected, actual);
            if (areEqual)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(AreEqual))} expected '{expected}' vs actual '{actual}': {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        [Conditional("PRG_ASSERT")]
        public static void AreNotEqual<T>(T expected, T actual, string message, object context, [CallerMemberName] string memberName = null)
        {
            var areEqual = EqualityComparer<T>.Default.Equals(expected, actual);
            if (!areEqual)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(AreNotEqual))} expected '{expected}' vs actual '{actual}': {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        [Conditional("PRG_ASSERT")]
        public static void IsNull<T>(T value, string message, object context, [CallerMemberName] string memberName = null) where T : class
        {
            var isNull = value == null;
            if (isNull)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(IsNull))}: {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        [Conditional("PRG_ASSERT")]
        public static void IsNotNull<T>(T value, string message, object context, [CallerMemberName] string memberName = null)
            where T : class
        {
            var isNull = value == null;
            if (!isNull)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(IsNotNull))}: {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        [Conditional("PRG_ASSERT")]
        public static void IsTrue(bool condition, string message, object context, [CallerMemberName] string memberName = null)
        {
            if (condition)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(IsTrue))}: {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        [Conditional("PRG_ASSERT")]
        public static void IsFalse(bool condition, string message, object context, [CallerMemberName] string memberName = null)
        {
            if (!condition)
            {
                return;
            }
            message = $"{RichText.Magenta(nameof(IsFalse))}: {message ?? DefaultError}";
            message = $"{RichText.Red(ErrorPrefix)} in {memberName}, {message}";
            FormatMessage(LogType.Error, message, AsContext(context), memberName, new StackFrame(1).GetMethod());
        }

        private static void FormatMessage(LogType logType, string message, Object context, string memberName, MethodBase method)
        {
#if PRG_ASSERT
            Debug.FormatMessage(logType, message, context, memberName, method);
#else
            UnityEngine.Debug.unityLogger.Log(logType, "", message, context);
#endif
        }
    }
}
