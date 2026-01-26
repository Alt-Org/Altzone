using System;
using System.Collections.Generic;
using Quantum;

namespace Battle.View
{
    public static class BattleViewRegistry
    {
        public static void Register(EntityRef e, QuantumEntityViewComponent view)
        {
            map[e] = view;
            if (pending.TryGetValue(e, out var list))
            {
                foreach (var cb in list) cb(view);
                pending.Remove(e);
            }
        }

        public static void Unregister(EntityRef e, QuantumEntityViewComponent view)
        {
            if(map.TryGetValue(e, out var cur) && cur == view) map.Remove(e);
        }

        public static bool TryGet(EntityRef e, out QuantumEntityViewComponent view) => map.TryGetValue(e, out view);

        public static void WhenRegistered(EntityRef e, Action<QuantumEntityViewComponent> callback)
        {
            if (TryGet(e, out var v)) { callback(v); return; }
            if (!pending.TryGetValue(e, out var list)) { list = new List<Action<QuantumEntityViewComponent>>(); pending[e] = list; }
            list.Add(callback);
        }

        private static readonly Dictionary<EntityRef, QuantumEntityViewComponent> map = new();
        private static readonly Dictionary<EntityRef, List<Action<QuantumEntityViewComponent>>> pending = new();
    }
}
