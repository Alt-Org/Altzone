///@file BattleViewRegistry.cs
///<summary>
///Contains @cref {Battle.View,BattleViewRegistry} class which handles mapping quantum entities
///to various objects they are related to.
///</summary>

//system usings
using System;
using System.Collections.Generic;

//quantum usings
using Quantum;

namespace Battle.View
{
    /// <summary>
    /// Handles mapping quantum entities to various objects they are related to.
    /// </summary>
    public static class BattleViewRegistry
    {
        /// <summary>
        /// Public method that handles mapping an object to a quantum entity.<br/>
        /// If the quantum entity has other objects registered to it, does callbacks to them.
        /// </summary>
        /// <param name="entityRef">Quantum entity the object is being mapped to.</param>
        /// <param name="object">object that is being mapped to the quantum entity.</param>
        public static void Register(EntityRef entityRef, object @object)
        {
            map[entityRef] = @object;
            if (pending.TryGetValue(entityRef, out List<Action<object>> list))
            {
                foreach (var callback in list) callback(@object);
                pending.Remove(entityRef);
            }
        }

        /*public static void Unregister(EntityRef entityRef, object @object)
        {
            if(map.TryGetValue(entityRef, out object cur) && cur == @object) map.Remove(entityRef);
        }*/

        /// <summary>
        /// Helper method for getting an object mapped to a quantum entity.
        /// </summary>
        /// <param name="entityRef">Quantum Entity being Get from.</param>
        /// <param name="object">Object trying to be Get.</param>
        /// <returns>Object mapped to entity if it exists.</returns>
        public static bool TryGet(EntityRef entityRef, out object @object) => map.TryGetValue(entityRef, out @object);

        /// <summary>
        /// Public method that handles adding callbacks for a quantum entity to a list.
        /// </summary>
        /// <param name="entityRef">Quantum entity callback needs to be mapped to.</param>
        /// <param name="callback">Callback that needs to be mapped to the quantum entity.</param>
        public static void WhenRegistered(EntityRef entityRef, Action<object> callback)
        {
            if (TryGet(entityRef, out var @object)) { callback(@object); return; }
            if (!pending.TryGetValue(entityRef, out List<Action<object>> list)) { list = new List<Action<object>>(); pending[entityRef] = list; }
            list.Add(callback);
        }

        /// <summary>
        /// Dictionary that holds the object that is mapped to each quantum entity.
        /// </summary>
        private static readonly Dictionary<EntityRef, object> map = new();

        /// <summary>
        /// Dictionary that holds a list of callbacks being mapped to each quantum entity.
        /// </summary>
        private static readonly Dictionary<EntityRef, List<Action<object>>> pending = new();
    }
}
