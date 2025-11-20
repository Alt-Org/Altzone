/// @file BattlePlayerClassManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassManager} class which handles player class logic.
/// Also contains @cref{Battle.QSimulation.Player,BattlePlayerClassBase} and @cref{Battle.QSimulation.Player,BattlePlayerClassBase<T>} classes
/// which player class implementations derive from.
/// </summary>

// System usings
using System.Collections.Generic;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Game;

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
            if (!f.Unsafe.TryGetPointer(playerEntity, out T* component))
            {
                Debug.LogErrorFormat("[PlayerCharacterClass] Class data ({0}) for {1} could not be found!", typeof(T).Name, playerEntity);
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
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnCreate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnCreate</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnSpawn method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnSpawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnSpawn</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnDespawn method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnDespawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnDespawn</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer reference to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Virtual OnProjectileHitPlayerCharacter method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnProjectileHitPlayerCharacter(Frame, BattleCollisionQSystem.ProjectileCollisionData*, BattleCollisionQSystem.PlayerCharacterCollisionData*)">
        /// OnProjectileHitPlayerHitbox
        /// </see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="playerCollisionData">Collision data related to the player character.</param>
        public virtual unsafe void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerCollisionData) { }

        /// <summary>
        /// Virtual OnProjectileHitPlayerShield method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnProjectileHitPlayerShield(Frame, BattleCollisionQSystem.ProjectileCollisionData*, BattleCollisionQSystem.PlayerShieldCollisionData*)">
        /// OnProjectileHitPlayerShield
        /// </see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public virtual unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData) { }

        /// <summary>
        /// Virtual OnUpdate method that can be implemented.<br/>
        /// Called by the public BattlePlayerClassManager
        /// <see cref="BattlePlayerClassManager.OnUpdate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnUpdate</see> method.
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
                    break;

                case BattlePlayerCharacterClass.Projector:
                    if (s_classArray[ClassIndexProjector] != null) break;
                    s_classArray[ClassIndexProjector] = new BattlePlayerClassProjector();
                    break;

                case BattlePlayerCharacterClass.Retroflector:
                    if (s_classArray[ClassIndexRetroflector] != null) break;
                    //s_classArray[ClassIndexRetroflector] = new BattlePlayerClassRetroflector();
                    break;

                case BattlePlayerCharacterClass.Confluent:
                    if (s_classArray[ClassIndexConfluent] != null) break;
                    s_classArray[ClassIndexConfluent] = new BattlePlayerClassConfluent();
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
        /// Calls the OnProjectileHitPlayerCharacter method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="playerCollisionData">Collision data related to the player character.</param>
        public static void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerCollisionData)
        {
            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerCollisionData->PlayerCharacterHitbox->PlayerEntity)->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnProjectileHitPlayerCharacter(f, projectileCollisionData, playerCollisionData);
        }

        /// <summary>
        /// Calls the OnProjectileHitPlayerShield method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public static void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(shieldCollisionData->PlayerShieldHitbox->PlayerEntity)->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnProjectileHitPlayerShield(f, projectileCollisionData, shieldCollisionData);
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
        /// <value>Constant for Projector class index.</value>
        private const int ClassIndexProjector = 2;
        /// <value>Constant for Retroflector class index.</value>
        private const int ClassIndexRetroflector = 3;
        /// <value>Constant for Confluent class index.</value>
        private const int ClassIndexConfluent = 4;
        /// <value>Constant for Intellectualizer class index.</value>
        private const int ClassIndexIntellectualizer = 5;
        /// <value>Constant for the amount of classes that exist.</value>
        private const int ClassCount = 6;

        /// <summary>
        /// Enum that defines the <see cref="BattlePlayerClassManager.GetClass">GetClass</see> method's return codes.<br/>
        /// Used to communicate the result of method call.
        /// </summary>
        private enum ReturnCode
        {
            /// <summary>A class was retrieved.</summary>
            ClassRetrieved = 0,

            /// <summary>
            /// No class was retrieved.<br/>
            /// Used when a character class has no code associated with it.
            /// </summary>
            NoClass = 1,

            /// <summary>Class could not be retrieved.</summary>
            Error = 2
        }

        /// <value>An array containing all of the class scripts that have been implemented and can be used.</value>
        private static BattlePlayerClassBase[] s_classArray = new BattlePlayerClassBase[ClassCount];

        /// <value>A dictionary used for tracking which classes have already had an error message sent regarding their missing implementation.</value>
        private static Dictionary<BattlePlayerCharacterClass, bool> s_errorMessagesSent = new();

        /// <summary>
        /// Returns the class script of the specified class from the class array, if it is implemented.
        /// </summary>
        ///
        /// <param name="characterClass">The player character class that's C# class is to be retrieved.</param>
        /// <param name="playerClass">The player character C# class that is retrieved. (<b>out param</b>)</param>
        ///
        /// <returns>Result of the method call as a <see cref="BattlePlayerClassManager.ReturnCode">ReturnCode</see>.</returns>
        private static ReturnCode GetClass(BattlePlayerCharacterClass characterClass, out BattlePlayerClassBase playerClass)
        {
            playerClass = null;

            int classIndex = characterClass switch
            {
                BattlePlayerCharacterClass.None             => ClassIndexNoCode,
                BattlePlayerCharacterClass.Desensitizer     => ClassIndexDesensitizer,
                //BattlePlayerCharacterClass.Trickster        => ClassIndexTrickster,
                BattlePlayerCharacterClass.Obedient         => ClassIndexNoCode,
                BattlePlayerCharacterClass.Projector        => ClassIndexProjector,
                //BattlePlayerCharacterClass.Retroflector     => ClassIndexRetroflector,
                BattlePlayerCharacterClass.Confluent        => ClassIndexConfluent,
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
