/// @file BattlePlayerClassManager.cs
/// <summary>
/// The manager script for player class logic.
/// </summary>
/// 
/// The manager handles initializing classes that are present in the game, and routing events forward to the individual classes.<br/>
/// This script also contains the base classes that class implementations derive from.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Battle.QSimulation.Game;
using Quantum;
using UnityEngine;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// An extended base abstact class for player classes.<br/>
    /// Contains a method for retrieving the class data component attached to the player character entity.<br/>
    /// Class scripts should extend this class if they make use of a class data component.
    /// </summary>
    public abstract unsafe class BattlePlayerClassBase<T> : BattlePlayerClassBase where T : unmanaged, IComponent
    {
        /// <summary>
        /// Returns a pointer reference to the class data of the specified player entity.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        /// <returns>A pointer reference to the player's class data.</returns>
        protected T* GetClassData(Frame f, EntityRef playerEntity)
        {
            if (!f.Unsafe.TryGetPointer<T>(playerEntity, out T* component))
            {
                Debug.LogErrorFormat("[PlayerCharacterClass] Class data for {0} could not be found!", playerEntity);
            }
            return component;
        }
    }

    /// <summary>
    /// The base abstract class for player classes.<br/>
    /// Contains a reference to the associated BattlePlayerCharacterClass enum and virtual methods for the different events the class can respond to.<br/>
    /// Class scripts should extend this class if they do not make use of a class data component.
    /// </summary>
    public abstract unsafe class BattlePlayerClassBase
    {
        /// <summary>
        /// Getter for the associated BattlePlayerCharacterClass enum of the class.
        /// </summary>
        public abstract BattlePlayerCharacterClass Class { get; }

        /// <summary>
        /// Virtual OnCreate method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnCreate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnCreate</see> method.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnSpawn method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnSpawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnSpawn</see> method.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnDespawn method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnDespawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnDespawn</see> method. 
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnProjectileHitPlayerHitbox method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnProjectileHitPlayerHitbox(Frame, BattleProjectileQComponent*, EntityRef, BattlePlayerHitboxQComponent*, EntityRef)">OnProjectileHitPlayerHitbox</see> method. 
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="data">Collision data.</param>
        public virtual unsafe void OnProjectileHitPlayerHitbox(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerData) { }

        /// <summary>
        /// Virtual OnProjectileHitPlayerShield method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnProjectileHitPlayerShield(Frame, BattleProjectileQComponent*, EntityRef, BattlePlayerHitboxQComponent*, EntityRef)">OnProjectileHitPlayerShield</see> method.  
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="data">Collision data.</param>
        public virtual unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldData) { }

        /// <summary>
        /// Virtual OnUpdate method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager <see cref="BattlePlayerClassManager.OnUpdate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnUpdate</see> method.  
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }
    }

    /// <summary>
    /// Handles the initial loading of player classes and routes individual game events to the correct class scripts.
    /// </summary>
    public unsafe static class BattlePlayerClassManager
    {

        /// <summary>
        /// Loads the specified class to be ready for use, if it is implemented.
        /// </summary>
        ///
        /// <param name="characterClass">The class that is to be loaded.</param>
        public static void LoadClass(BattlePlayerCharacterClass characterClass)
        {
            switch (characterClass)
            {
                case BattlePlayerCharacterClass.Desensitizer:
                    if (s_classArray[ClassIndexDesensitizer] != null) break;
                    s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassDesensitizer();
                    break;

                case BattlePlayerCharacterClass.Trickster:
                    if (s_classArray[ClassIndexTrickster] != null) break;
                    //s_classArray[ClassIndexTrickster] = new BattlePlayerClassTrickster();
                    break;

                case BattlePlayerCharacterClass.Obedient:
                    if (s_classArray[ClassIndexObedient] != null) break;
                    //s_classArray[ClassIndexObedient] = new BattlePlayerClassObedient();
                    break;

                case BattlePlayerCharacterClass.Projector:
                    if (s_classArray[ClassIndexProjector] != null) break;
                    //s_classArray[ClassIndexProjector] = new BattlePlayerClassProjector();
                    break;

                case BattlePlayerCharacterClass.Retroflector:
                    if (s_classArray[ClassIndexRetroflector] != null) break;
                    //s_classArray[ClassIndexRetroflector] = new BattlePlayerClassRetroflector();
                    break;

                case BattlePlayerCharacterClass.Confluent:
                    if (s_classArray[ClassIndexConfluent] != null) break;
                    //s_classArray[ClassIndexConfluent] = new BattlePlayerClassConfluent();
                    break;

                case BattlePlayerCharacterClass.Intellectualizer:
                    if (s_classArray[ClassIndexIntellectualizer] != null) break;
                    //s_classArray[ClassIndexIntellectualizer] = new BattlePlayerClassIntellectualizer();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Calls the OnCreate method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public static void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode == ReturnCode.Error)
            {
                Debug.LogWarningFormat("[PlayerClassManager] The {0} class could not be initialized!", playerData->CharacterClass);
                return;
            }
            if (returnCode == ReturnCode.NoClass)
            {
                return;
            }

            playerClass.OnCreate(f, playerHandle, playerData, playerEntity);
        }

        /// <summary>
        /// Calls the OnSpawn method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public static void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnSpawn(f, playerHandle, playerData, playerEntity);
        }

        /// <summary>
        /// Calls the OnDespawn method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public static void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnDespawn(f, playerHandle, playerData, playerEntity);
        }

        /// <summary>
        /// Calls the OnProjectileHitPlayerHitbox method of the class of the given player character, if it is implemented.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileData">Collision data related to the projectile.</param>
        /// <param name="playerData">Collision data related to the player character.</param>
        public static void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerData)
        {
            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerData->PlayerHitbox->PlayerEntity)->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnProjectileHitPlayerHitbox(f, projectileData, playerData);
        }

        /// <summary>
        /// Calls the OnProjectileHitPlayerShield method of the class of the given player character, if it is implemented.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileData">Collision data related to the projectile.</param>
        /// <param name="shieldData">Collision data related to the player shield.</param>
        public static void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldData)
        {
            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(shieldData->PlayerShieldHitbox->PlayerEntity)->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnProjectileHitPlayerShield(f, projectileData, shieldData);
        }

        /// <summary>
        /// Calls the OnUpdate method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public static void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnUpdate(f, playerHandle, playerData, playerEntity);
        }

        /// <value>Constant for a class index error.</value>
        private const int ClassIndexError = -2;
        /// <value>Constant for a no code class index.</value>
        private const int ClassIndexNoCode = -1;
        /// <value>Constant for Desensitizer class index.</value>
        private const int ClassIndexDesensitizer = 0;
        /// <value>Constant for Trickster class index.</value>
        private const int ClassIndexTrickster = 1;
        /// <value>Constant for Obedient class index.</value>
        private const int ClassIndexObedient = 2;
        /// <value>Constant for Projector class index.</value>
        private const int ClassIndexProjector = 3;
        /// <value>Constant for Retroflector class index.</value>
        private const int ClassIndexRetroflector = 4;
        /// <value>Constant for Confluent class index.</value>
        private const int ClassIndexConfluent = 5;
        /// <value>Constant for Intellectualizer class index.</value>
        private const int ClassIndexIntellectualizer = 6;
        /// <value>Constant for the amount of classes that exist.</value>
        private const int ClassCount = 7;

        /// <summary>Enum for the different values GetClass can return.</summary>
        private enum ReturnCode
        {
            ClassRetrieved = 0,
            NoClass        = 1,
            Error          = 2
        }

        /// <value>An array containing all of the class scripts that have been implemented and can be used.</value>
        private static BattlePlayerClassBase[] s_classArray = new BattlePlayerClassBase[ClassCount];

        /// <value>A dictionary used for tracking which classes have already had an error message sent regarding their missing implementation.</value>
        private static Dictionary<BattlePlayerCharacterClass, bool> s_errorMessagesSent = new();

        /// <summary>
        /// Returns the class script of the specified class from the class array, if it is implemented.
        /// </summary>
        ///
        /// <param name="characterClass">The class that's script is to be retrieved.</param>
        private static ReturnCode GetClass(BattlePlayerCharacterClass characterClass, out BattlePlayerClassBase playerClass)
        {
            playerClass = null;

            int classIndex = characterClass switch
            {
                BattlePlayerCharacterClass.None             => ClassIndexNoCode,
                BattlePlayerCharacterClass.Desensitizer     => ClassIndexDesensitizer,
                //BattlePlayerCharacterClass.Trickster        => ClassIndexTrickster,
                //BattlePlayerCharacterClass.Obedient         => ClassIndexObedient,
                //BattlePlayerCharacterClass.Projector        => ClassIndexProjector,
                //BattlePlayerCharacterClass.Retroflector     => ClassIndexRetroflector,
                //BattlePlayerCharacterClass.Confluent        => ClassIndexConfluent,
                //BattlePlayerCharacterClass.Intellectualizer => ClassIndexIntellectualizer,

                _ => ClassIndexError,
            };

            if (classIndex == ClassIndexNoCode) return ReturnCode.NoClass;

            if (classIndex == ClassIndexError)
            {
                if (!s_errorMessagesSent.ContainsKey(characterClass))
                {
                    s_errorMessagesSent.Add(characterClass, false);
                }

                if (!s_errorMessagesSent[characterClass])
                {
                    Debug.LogWarningFormat("[PlayerClassManager] The {0} class is not currently implemented!", characterClass);
                    s_errorMessagesSent[characterClass] = true;
                }

                return ReturnCode.Error;
            }

            BattlePlayerClassBase classObj = s_classArray[classIndex];

            if (classObj == null)
            {
                Debug.LogErrorFormat("[PlayerClassManager] The {0} class is not in the class array!", characterClass);
                return ReturnCode.Error;
            }

            if (classObj.Class != characterClass)
            {
                Debug.LogErrorFormat("[PlayerClassManager] Mapping of character classes is incorrect! Expected {0}, got {1}", characterClass, classObj.Class);
                return ReturnCode.Error;
            }

            playerClass = classObj;
            return ReturnCode.ClassRetrieved;
        }
    }
}
