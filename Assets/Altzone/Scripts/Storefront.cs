using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Factory class for our <c>DataStore</c> and support for internal game update/upgrade operations.
    /// </summary>
    public static class Storefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static DataStore _instance;

        /// <summary>
        /// Gets or creates an <c>DataStore</c> static singleton instance. 
        /// </summary>
        public static DataStore Get() => _instance ??= new DataStore();
    }

    /// <summary>
    /// Public <c>DataStore</c> interface.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DataStore
    {
        private const string StorageFilename = "LocalModels.json";

        private readonly LocalModels _localModels;

        public DataStore()
        {
            _localModels = new LocalModels(StorageFilename);
        }

        /// <summary>
        /// Gets <c>PlayerData</c> entity using its <c>uniqueIdentifier</c> property.
        /// </summary>
        /// <remarks>
        /// Note that <c>uniqueIdentifier</c> is not the same as ID but should be generated once for given device so that
        /// new player can be identified unambiguously when it is created for first time.
        /// </remarks>
        public void GetPlayerData(string uniqueIdentifier, Action<PlayerData> callback)
        {
            _localModels.GetPlayerData(uniqueIdentifier, callback);
        }

        /// <summary>
        /// Saves <c>PlayerData</c> entity.
        /// </summary>
        /// <remarks>
        /// If <c>PlayerData</c> was created its ID will be updated in returned entity.
        /// </remarks>
        public void SavePlayerData(PlayerData playerData, Action<PlayerData> callback)
        {
            _localModels.SavePlayerData(playerData, callback);
        }

        /// <summary>
        /// Gets <c>ClanData</c> entity using its ID property.
        /// </summary>
        public void GetClanData(string id, Action<ClanData> callback)
        {
            _localModels.GetClanData(id, callback);
        }

        /// <summary>
        /// Get all read-only <c>GameFurniture</c> entities.
        /// </summary>
        /// <returns><c>CustomYieldInstruction</c> that can be 'waited' in UNITY <c>Coroutine</c> using <code>yield return</code></returns>
        public CustomYieldInstruction GetAllGameFurnitureYield(Action<ReadOnlyCollection<GameFurniture>> callback)
        {
            return new MYCustomYieldInstruction(_localModels, callback);
        }

        private class MYCustomYieldInstruction : CustomYieldInstruction
        {
            public override bool keepWaiting => _keepWaiting;

            private bool _keepWaiting = true;

            public MYCustomYieldInstruction(LocalModels localModels, Action<ReadOnlyCollection<GameFurniture>> callback)
            {
                void SafeCallbackWrapper(ReadOnlyCollection<GameFurniture> result)
                {
                    try
                    {
                        callback(result);
                    }
                    finally
                    {
                        _keepWaiting = false;
                    }
                }

                localModels.GetAllGameFurniture(SafeCallbackWrapper);
            }
        }
    }
}
