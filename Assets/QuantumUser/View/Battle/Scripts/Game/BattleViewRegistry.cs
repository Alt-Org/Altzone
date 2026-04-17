/// @file BattleViewRegistry.cs
/// <summary>
/// Contains @cref{Battle.View,BattleViewRegistry} class which handles mapping %Quantum entities
/// to various objects they are related to.
/// </summary>

// system usings
using System;
using System.Collections.Generic;

// quantum usings
using Quantum;

using UnityEngine;

namespace Battle.View
{
    /// <summary>
    /// Provides a way to link %Quantum entities on the view side of %Quantum
    /// </summary>
    ///
    /// @anchor BattleViewRegistry-MethodsDoc
    /// @bigtext{**Methods Overview**}
    ///
    /// **Register**<br/>
    /// The Register method handles mapping an object to the %Quantum entity that owns it, and then calls all callbacks it may have mapped beforehand.<br/>
    ///
    /// **Example:**<br/>
    /// ```cs
    /// class ExampleObject
    /// {
    ///     ...
    ///     public override void OnActivate(Frame _)
    ///     {
    ///         ...
    ///         BattleViewRegistry.Register(EntityRef, this);
    ///         ...
    ///     }
    ///     ...
    /// }
    /// ```
    ///
    /// **WhenRegistered**<br/>
    /// The WhenRegistered method handles adding callbacks for a %Quantum entity to a list that gets called when the entity is registered.
    ///
    /// **Example:**<br/>
    /// ```cs
    /// public override void OnActivate(Frame _)
    /// {
    ///     ...
    ///     BattleViewRegistry.WhenRegistered(characterRef, ExampleMethod);
    ///     ...
    /// }
    /// ...
    /// private void ExampleMethod()
    /// {
    ///      ...
    /// }
    /// ```
    ///
    /// **Get methods**<br/>
    /// The get methods are getter methods that get either a single object or a list of objects registered to a %Quantum entity.
    ///
    /// **Example:**<br/>
    /// ```cs
    /// private void ExampleMethod()
    /// {
    ///     BattlePlayerCharacterViewController example = BattleViewRegistry.GetObject<BattlePlayerCharacterViewController>(entityRef);
    ///     List<BattlePlayerCharacterViewController> exampleList = BattleViewRegistry.GetObjects<BattlePlayerCharacterViewController(entityRef);
    /// }
    /// ```
    public static class BattleViewRegistry
    {
        /// <summary>
        /// Registers an <paramref name="object"/> to a %Quantum <paramref name="entityRef"/>.<br/>
        /// If the %Quantum <paramref name="entityRef"/> has other objects registered to it, does callbacks to them.
        /// </summary>
        ///
        /// <param name="entityRef">%Quantum entity the object is being mapped to.</param>
        /// <param name="object">object that is being mapped to the %Quantum entity.</param>
        public static void Register(EntityRef entityRef, object @object)
        {
            RegistryEntry entry = GetEntryOrNew(entityRef);

            entry.Registered = true;
            entry.Objects.Add(@object);
            HandleCallbacks(entry.PendingCallbacks);
        }

        /// <summary>
        /// Fetches an object of type <typeparamref name="T"/> mapped to a %Quantum <paramref name="entityRef"/>.
        /// </summary>
        ///
        /// <typeparam name="T">Type of object trying to be fetched.</typeparam>
        /// <param name="entityRef">%Quantum Entity being fetched from.</param>
        ///
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
        /// Fetches a list of all objects of type <typeparamref name="T"/> mapped to a %Quantum <paramref name="entityRef"/>.
        /// </summary>
        ///
        /// <typeparam name="T">Type of objects trying to be fetched.</typeparam>
        /// <param name="entityRef">%Quantum Entity being fetched from.</param>
        ///
        /// <returns>a list of objects if it exists.</returns>
        public static List<T> GetObjects<T>(EntityRef entityRef) where T : class
        {
            if (!TryGetEntry(entityRef, out RegistryEntry entry)) return null;

            List<T> results = new();

            foreach(object obj in entry.Objects)
            {
                if (obj is T match) results.Add(match);
            }

            return results;
        }

        /// <summary>
        /// Adds <paramref name="callback"/> to a %Quantum <paramref name="entityRef"/>.
        /// </summary>
        ///
        /// <param name="entityRef">%Quantum entity callback needs to be mapped to.</param>
        /// <param name="callback">Callback that needs to be mapped to the %Quantum entity.</param>
        public static void WhenRegistered(EntityRef entityRef, Func<bool> callback)
        {
            RegistryEntry entry = GetEntryOrNew(entityRef);
            if (entry.Registered)
            {
                if (callback()) return;
            }
            entry.PendingCallbacks.Add(new Func<bool>(callback));
        }

        /// <summary>
        /// Class that holds all information needed for a registry entry.
        /// </summary>
        private class RegistryEntry
        {
            /// <summary>Boolean for whether this entry is registered or not.</summary>
            public bool Registered;

            /// <summary>List of objects associated with the entry.</summary>
            public List<object> Objects;

            /// <summary>List of RegistryCallbacks associated with the entry.</summary>
            public List<Func<bool>> PendingCallbacks;

            /// <summary>Constructor for RegistryEntry.</summary>
            public RegistryEntry()
            {
                Registered = false;
                Objects = new List<object>();
                PendingCallbacks = new List<Func<bool>>();
            }
        }

        /// <summary>Dictionary that holds a list of RegistryEntries mapped to each %Quantum entity.</summary>
        private static readonly Dictionary<EntityRef, RegistryEntry> s_registryMap = new();

        /// <summary>
        /// Private helper method that fetches a registry entry mapped to a specific %Quantum <paramref name="entityRef"/> or adds one if it doesn't exist.
        /// </summary>
        ///
        /// <param name="entityRef">%Quantum entity the entry is mapped to.</param>
        ///
        /// <returns>The entry mapped to the %Quantum entity.</returns>
        private static RegistryEntry GetEntryOrNew(EntityRef entityRef)
        {
            if (!s_registryMap.TryGetValue(entityRef, out RegistryEntry entry))
            {
                entry = new RegistryEntry();
                s_registryMap[entityRef] = entry;
            }
            return entry;
        }

        /// <summary>
        /// Private helper method for fetching an <paramref name="entry"/> mapped to a %Quantum <paramref name="entityRef"/> if it exists.
        /// </summary>
        ///
        /// <param name="entityRef">%Quantum entity trying to be fetched from.</param>
        /// <param name="entry">Entry that is returned as an out parameter.</param>
        ///
        /// <returns>True if fetching succeeded, false if not.</returns>
        private static bool TryGetEntry(EntityRef entityRef, out RegistryEntry entry)
        {
            return s_registryMap.TryGetValue(entityRef, out entry);
        }

        /// <summary>
        /// Private helper method for handling <paramref name="callbacks"/>.
        /// </summary>
        ///
        /// <param name="callbacks">List of callbacks that need to be handled.</param>
        private static void HandleCallbacks(List<Func<bool>> callbacks)
        {
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < callbacks.Count; readIndex++)
            {
                Func<bool> callback = callbacks[readIndex];
                if (callback()) continue;

                callbacks[writeIndex] = callback;
                writeIndex++;
            }
            int removeCount = callbacks.Count - writeIndex;
            if (removeCount > 0)
            {
                callbacks.RemoveRange(writeIndex, removeCount);
            }
        }
    }
}
