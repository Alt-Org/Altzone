using System.Runtime.CompilerServices;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal class BattleDebugLogger
    {
        public static void Init(SyncedFixedUpdateClock syncedFixedUpdateClock)
        {
            s_syncedFixedUpdateClock = syncedFixedUpdateClock;
            Debug.Log("[[BATTLE LOG START]]");
        }

        public BattleDebugLogger(object source)
        {
            _loggerFormat = "[{0:000000}] [BATTLE] [" + source.GetType().Name + "] {1}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogInfo(string msg) { Debug.Log(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogInfo(string msgFormat, params object[] args) { LogInfo(string.Format(msgFormat, args)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogWarning(string msg) { Debug.LogWarning(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogWarning(string msgFormat, params object[] args) { LogWarning(string.Format(msgFormat, args)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogError(string msg) { Debug.LogError(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogError(string msgFormat, params object[] args) { LogError(string.Format(msgFormat, args)); }

        private static SyncedFixedUpdateClock s_syncedFixedUpdateClock;

        private readonly string _loggerFormat;
    }
}
