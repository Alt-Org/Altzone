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
            if(!map.TryGetValue(entityRef, out List<object> _list)) {
                _list = new List<object>();
                map[entityRef] = _list;
            }

            map[entityRef].Add(@object);

            if (pending.TryGetValue(entityRef, out List<Action> list))
            {
                foreach (var callback in list) callback();
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
        /// <typeparam name="T">type of object trying to be Got.</typeparam>
        /// <param name="entityRef">Quantum Entity being Get from.</param>
        /// <returns>Object mapped to entity if it exists.</returns>
        public static T GetObject<T>(EntityRef entityRef) where T : class
        {
            if(!map.TryGetValue(entityRef, out List<object> list))
            {
                return default(T);
            }

            foreach (var obj in list)
                if (obj is T match)
                    return match;
            return default(T);
        }

        /// <summary>
        /// Helper method for getting a list of objects mapped to a quantum entity.
        /// </summary>
        /// <typeparam name="T">type of objects trying to be Got.</typeparam>
        /// <param name="entityRef">Quantum Entity being Get from.</param>
        /// <returns>a list of objects if it exists.</returns>
        public static List<T> GetObjects<T>(EntityRef entityRef) where T : class
        {
            if (!map.TryGetValue(entityRef, out List<object> list))
            {
                return default(List<T>);
            }

            List<T> results = new List<T>();

            foreach(var obj in list)
                if(obj is T match)
                    results.Add(match);
            return results;
        }

        /// <summary>
        /// Public method that handles adding callbacks for a quantum entity to a list.
        /// </summary>
        /// <param name="entityRef">Quantum entity callback needs to be mapped to.</param>
        /// <param name="callback">Callback that needs to be mapped to the quantum entity.</param>
        public static void WhenRegistered(EntityRef entityRef, Action callback)
        {
            if (map.TryGetValue(entityRef, out List<object> list)){ callback(); return;}

            if(!pending.TryGetValue(entityRef, out List<Action> callbacks))
            {
                callbacks = new List<Action>();
                pending[entityRef] = callbacks;
            }
            callbacks.Add(callback);
        }

        /// <summary>
        /// Dictionary that holds the object that is mapped to each quantum entity.
        /// </summary>
        private static readonly Dictionary<EntityRef, List<object>> map = new();

        /// <summary>
        /// Dictionary that holds a list of callbacks being mapped to each quantum entity.
        /// </summary>
        private static readonly Dictionary<EntityRef, List<Action>> pending = new();
    }
}
