// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

namespace Battle.QSimulation
{
    public class BattleDebugLogger
    {
        #region Create Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create<T>()
        {
            return new BattleDebugLogger(typeof(T).Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create(System.Type source)
        {
            return new BattleDebugLogger(source.Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create(string source)
        {
            return new BattleDebugLogger(source);
        }

        #endregion Create Methods

        #region Public Static Log Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string source, string message)
        {
            Debug.LogFormat(StaticFormatNoFrame, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(Frame f, string source, string message)
        {
            Debug.LogFormat(StaticFormat, f.Number, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(string source, string format, params object[] args)
        {
            Log(source, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(Frame f, string source, string format, params object[] args)
        {
            Log(f, source, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string source, string message)
        {
            Debug.LogWarningFormat(StaticFormatNoFrame, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Frame f, string source, string message)
        {
            Debug.LogWarningFormat(StaticFormat, f.Number, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(string source, string format, params object[] args)
        {
            Warning(source, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(Frame f, string source, string format, params object[] args)
        {
            Warning(f, source, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string source, string message)
        {
            Debug.LogErrorFormat(StaticFormatNoFrame, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Frame f, string source, string message)
        {
            Debug.LogErrorFormat(StaticFormat, f.Number, source, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(string source, string format, params object[] args)
        {
            Error(source, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(Frame f, string source, string format, params object[] args)
        {
            Error(f, source, string.Format(format, args));
        }

        #endregion Public Static Log Methods

        #region Public Log Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(string message)
        {
            Debug.LogFormat(_formatNoFrame, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(Frame f, string message)
        {
            Debug.LogFormat(_format, f.Number, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(Frame f, string format, params object[] args)
        {
            Log(f, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message)
        {
            Debug.LogWarningFormat(_formatNoFrame, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(Frame f, string message)
        {
            Debug.LogWarningFormat(_format, f.Number, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(string format, params object[] args)
        {
            Warning(string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(Frame f, string format, params object[] args)
        {
            Warning(f, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message)
        {
            Debug.LogErrorFormat(_formatNoFrame, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Frame f, string message)
        {
            Debug.LogErrorFormat(_format, f.Number, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(Frame f, string format, params object[] args)
        {
            Error(f, string.Format(format, args));
        }

        #endregion Public Log Methods

        private const string StaticFormat                  = "[Battle] [{0:D6}] [{1}] {2}";
        private const string StaticFormatNoFrame           = "[Battle] [{0}] {1}";
        private const string InstanceFormatTemplate        = "[Battle] [{{0:D6}}] [{0}] {{1}}";
        private const string InstanceFormatNoFrameTemplate = "[Battle] [{0}] {{0}}";

        private readonly string _format;
        private readonly string _formatNoFrame;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BattleDebugLogger(string source)
        {
            _format        = string.Format(InstanceFormatTemplate, source);
            _formatNoFrame = string.Format(InstanceFormatNoFrameTemplate, source);
        }
    }
}
