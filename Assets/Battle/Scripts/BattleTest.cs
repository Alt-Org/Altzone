using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts
{
    internal static class BattleTest
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            Debug.Log($"Photon realtime {RealtimeClient.Version} for Quantum");
        }
    }
}
