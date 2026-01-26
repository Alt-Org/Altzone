using System;
using System.Collections.Generic;
using Quantum;

namespace Battle.View
{
    public static class BattleViewRegistry
    {
        public static void Register(EntityRef entityRef, object @object)
        {
            map[entityRef] = @object;
            if (pending.TryGetValue(entityRef, out var list))
            {
                foreach (var callback in list) callback(@object);
                pending.Remove(entityRef);
            }
        }

        public static void Unregister(EntityRef entityRef, object @object)
        {
            if(map.TryGetValue(entityRef, out var cur) && cur == @object) map.Remove(entityRef);
        }

        public static bool TryGet(EntityRef entityRef, out object @object) => map.TryGetValue(entityRef, out @object);

        public static void WhenRegistered(EntityRef entityRef, Action<object> callback)
        {
            if (TryGet(entityRef, out var @object)) { callback(@object); return; }
            if (!pending.TryGetValue(entityRef, out var list)) { list = new List<Action<object>>(); pending[entityRef] = list; }
            list.Add(callback);
        }

        private static readonly Dictionary<EntityRef, object> map = new();
        private static readonly Dictionary<EntityRef, List<Action<object>>> pending = new();
    }
}
