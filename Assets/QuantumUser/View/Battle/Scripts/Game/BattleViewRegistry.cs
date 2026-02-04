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
            RegistryEntry entry = GetEntryOrNew(entityRef);

            entry.Registered = true;
            entry.Objects.Add(@object);
            HandleCallbacks(entry.PendingCallbacks);
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
            if(!TryGetEntry(entityRef, out RegistryEntry entry)) return null;

            foreach (object obj in entry.Objects)
            {
                if (obj is T match) return match;
            }
            return null;
        }

        /// <summary>
        /// Helper method for getting a list of objects mapped to a quantum entity.
        /// </summary>
        /// <typeparam name="T">type of objects trying to be Got.</typeparam>
        /// <param name="entityRef">Quantum Entity being Get from.</param>
        /// <returns>a list of objects if it exists.</returns>
        public static List<T> GetObjects<T>(EntityRef entityRef) where T : class
        {
            if (!TryGetEntry(entityRef, out RegistryEntry entry)) return null;

            List<T> results = new List<T>();

            foreach(object obj in entry.Objects)
            {
                if (obj is T match) results.Add(match);
            }

            return results;
        }

        /// <summary>
        /// Public method that handles adding callbacks for a quantum entity to a list.
        /// </summary>
        /// <param name="entityRef">Quantum entity callback needs to be mapped to.</param>
        /// <param name="callback">Callback that needs to be mapped to the quantum entity.</param>
        public static void WhenRegistered(EntityRef entityRef, Func<bool> callback)
        {
            RegistryEntry entry = GetEntryOrNew(entityRef);
            if (entry.Registered)
            {
                if (callback()) return;
            }
            entry.PendingCallbacks.Add(new RegistryCallback(callback));
        }

        /// <summary>
        /// Class that holds all information needed for a registry entry.
        /// </summary>
        private class RegistryEntry
        {
            public bool Registered;
            public List<object> Objects;
            public List<RegistryCallback> PendingCallbacks;

            public RegistryEntry()
            {
                Registered = false;
                Objects = new List<object>();
                PendingCallbacks = new List<RegistryCallback>();
            }
        }

        /// <summary>
        /// Struct that holds all information needed for callbacks.
        /// </summary>
        private struct RegistryCallback
        {
            public bool Handled;
            public Func<bool> Callback;

            public RegistryCallback(Func<bool> callback)
            {
                Handled = false;
                Callback = callback;
            }
        }

        /// <summary>Dictionary that holds a list of RegistryEntries mapped to each quantum entity.</summary>
        private static readonly Dictionary<EntityRef, RegistryEntry> _registryMap = new();

        /// <summary>
        /// Helper method that gets a registry entry mapped to a specific Quantum entity or adds one if it doesn't exist.
        /// </summary>
        ///
        /// <param name="entityRef">Quantum entity the entry is mapped to.</param>
        ///
        /// <returns>The entry mapped to the Quantum entity.</returns>
        private static RegistryEntry GetEntryOrNew(EntityRef entityRef)
        {
            if (!_registryMap.TryGetValue(entityRef, out RegistryEntry entry))
            {
                entry = new RegistryEntry();
                _registryMap[entityRef] = entry;
            }
            return entry;
        }

        /// <summary>
        /// Dictionary that holds the object that is mapped to each quantum entity.
        /// </summary>

        /// <summary>
        /// Dictionary that holds a list of callbacks being mapped to each quantum entity.
        /// </summary>
    }
}
