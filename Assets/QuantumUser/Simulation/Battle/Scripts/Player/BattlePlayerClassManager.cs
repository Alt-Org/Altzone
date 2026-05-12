/// @file BattlePlayerClassManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassManager} class which handles player class logic.
/// Also contains @cref{Battle.QSimulation.Player,BattlePlayerClassBase} and @cref{Battle.QSimulation.Player,BattlePlayerClassBase<T>} classes
/// which player class implementations derive from.
/// </summary>

// System usings
using System.Collections.Generic;

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
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    public abstract unsafe class BattlePlayerClassBase<T> : BattlePlayerClassBase where T : unmanaged, IComponent
    {
        /// <summary>
        /// Returns a pointer to the class data of the specified player entity.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        /// <returns>A pointer to the player's class data.</returns>
        protected T* GetClassData(Frame f, EntityRef playerEntity)
        {
            if (!f.Unsafe.TryGetPointer(playerEntity, out T* component))
            {
                BattleDebugLogger.ErrorFormat(f, "BattlePlayerClassBase", "Class data ({0}) for {1} could not be found!", typeof(T).Name, playerEntity);
            }
            return component;
        }
    }

    /// <summary>
    /// The base abstract class for player classes.<br/>
    /// Contains a reference to the associated BattlePlayerCharacterClass enum and virtual methods for the different events the class can respond to.<br/>
    /// Class scripts should extend this class if they do not make use of a class data component.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    public abstract unsafe class BattlePlayerClassBase
    {
        /// <summary>
        /// Getter for the associated BattlePlayerCharacterClass enum of the class.
        /// </summary>
        public abstract BattlePlayerCharacterClass Class { get; }

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnCreate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnCreate</see> method
        /// when character gets created at the start of the game.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        ///
        /// <returns>Default @cref{Battle.QSimulation.Player.BattlePlayerClassManager,CreationParameters} if not overwritten.</returns>
        public virtual unsafe BattlePlayerClassManager.CreationParameters OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) => BattlePlayerClassManager.CreationParameters.Default;

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnSpawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnSpawn</see> method
        /// each time character is spawned.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnDespawn(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, EntityRef)">OnDespawn</see> method
        /// each time character is despawned.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnProjectileHitPlayerCharacter(Frame, BattleCollisionQSystem.ProjectileCollisionData*, BattleCollisionQSystem.PlayerCharacterCollisionData*)">
        /// OnProjectileHitPlayerHitbox
        /// </see> method when projectile hits this character.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="playerCollisionData">Collision data related to the player character.</param>
        public virtual unsafe void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerCollisionData) { }

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnProjectileHitPlayerShield(Frame, BattleCollisionQSystem.ProjectileCollisionData*, BattleCollisionQSystem.PlayerShieldCollisionData*)">
        /// OnProjectileHitPlayerShield
        /// </see> method when projectile hits this shield.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public virtual unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData) { }

        /// <summary>
        /// Called by the @cref{Battle.QSimulation.Player,BattlePlayerClassManager}
        /// <see cref="BattlePlayerClassManager.OnUpdate(Frame, BattlePlayerManager.PlayerHandle, BattlePlayerDataQComponent*, BattlePlayerEntityRef)">OnUpdate</see> method
        /// once per frame.<br/>
        /// Provides a hook for derived classes to implement character class specific simulation logic.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public virtual unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, BattlePlayerEntityRef playerEntity) { }
    }

    /// <summary>
    /// Handles the initial loading of player classes and routes individual game events to the correct class scripts.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClassManager}](#page-concepts-player-simulation-class-classmanager) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Overview}](#page-concepts-player-overview) for more info.}<br/>
    /// @bigtext{See [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview) for more info.}<br/>
    public static unsafe class BattlePlayerClassManager
    {
        /// <summary>
        /// Gives class code access to creation parameters.
        /// </summary>
        public struct CreationParameters
        {
            /// <summary>
            /// Default parameters.
            /// </summary>
            public static CreationParameters Default => new()
            {
                AttachedShieldNumber = 0
            };

            /// <summary>
            /// The ShieldNumber of the attached shield. -1 is none.
            /// </summary>
            public int AttachedShieldNumber;
        }

        /// <summary>
        /// Enum that defines the <see cref="PickClass"/> method's return options.
        /// </summary>
        public enum ClassOption
        {
            /// <summary>Use no class script</summary>
            UseNone,
            /// <summary>Use class script</summary>
            UseClass,
            /// <summary>Use test class script</summary>
            UseTestClass
        }

        /// <summary>
        /// Initializes this classes BattleDebugLogger instance and fills <see cref="s_classArray"/> states with <see cref="ClassState.Notloaded"/>.<br/>
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create(typeof(BattlePlayerClassManager));

            for (int i = 0; i < s_classArray.Length; i++)
            {
                s_classArray[i].State = ClassState.NotLoaded;
            }
        }

        /// <summary>
        /// Loads the specified class to be ready for use, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="characterClass">The class that is to be loaded.</param>
        public static void LoadClass(Frame f, BattlePlayerCharacterClass characterClass)
        {
            bool isTestMode = BattleParameters.GetIsTestMode(f);

            switch (characterClass)
            {
                case BattlePlayerCharacterClass.Class100:
                    if (s_classArray[ClassIndex100].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex100, new BattlePlayerClass100(), null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class200:
                    if (s_classArray[ClassIndex200].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex200, null, null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class300:
                    if (s_classArray[ClassIndex300].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex300, null, null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class400:
                    if (s_classArray[ClassIndex400].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex400, new BattlePlayerClass400(), null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class500:
                    if (s_classArray[ClassIndex500].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex500, null, null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class600:
                    if (s_classArray[ClassIndex600].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex600, new BattlePlayerClass600(), null, isTestMode);
                    break;

                case BattlePlayerCharacterClass.Class700:
                    if (s_classArray[ClassIndex700].State != ClassState.NotLoaded) break;
                    LoadClass(characterClass, ClassIndex700, null, null, isTestMode);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Decides which <see cref="ClassOption"/> to use depending on available class scripts and whether <paramref name="testMode"/> is enabled.
        /// </summary>
        ///
        /// See [PlayerClass](#page-concepts-player-simulation-class-playerclass) for more info<br/>
        /// See [Player Character Classes](#page-concepts-player-characters-classes) for more info
        ///
        /// |                                       | has none                   | <paramref name="hasClass"/> | <paramref name="hasTestClass"/> | has both                        |
        /// | :------------------------------------ | :------------------------- | :-------------------------- | :------------------------------ | :------------------------------ |
        /// | <paramref name="testMode"/> **false** | @cref{ClassOption,UseNone} | @cref{ClassOption,UseClass} | @cref{ClassOption,UseNone}      | @cref{ClassOption,UseClass}     |
        /// | <paramref name="testMode"/> **true**  | @cref{ClassOption,UseNone} | @cref{ClassOption,UseClass} | @cref{ClassOption,UseTestClass} | @cref{ClassOption,UseTestClass} |
        ///
        /// <param name="hasClass">Normal class script reference to check</param>
        /// <param name="hasTestClass">Test class script reference to check</param>
        /// <param name="testMode">Is test mode enabled</param>
        ///
        /// <returns>The picked <see cref="ClassOption"/></returns>
        public static ClassOption PickClass(object hasClass, object hasTestClass, bool testMode)
        {
            ClassOption useClass = hasClass != null ? ClassOption.UseClass : ClassOption.UseNone;

            if (!testMode)
            {
                return useClass;
            }
            else
            {
                return hasTestClass != null ? ClassOption.UseTestClass : useClass;
            }
        }

        /// <summary>
        /// Calls the OnCreate method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        ///
        /// <returns>@cref{Battle.QSimulation.Player.BattlePlayerClassManager,CreationParameters}.</returns>
        public static CreationParameters OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode == ReturnCode.Error)
            {
                s_debugLogger.WarningFormat("The {0} class could not be initialized!", playerData->CharacterClass);
                return CreationParameters.Default;
            }
            if (returnCode == ReturnCode.NoClass)
            {
                return CreationParameters.Default;
            }

            return playerClass.OnCreate(f, playerHandle, playerData, playerEntity);
        }

        /// <summary>
        /// Calls the OnSpawn method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
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
        /// <param name="playerData">Pointer to the player data.</param>
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
            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerCollisionData->PlayerCharacterHitbox->ParentEntityRef)->CharacterClass, out BattlePlayerClassBase playerClass);

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
            BattlePlayerShieldDataQComponent* playerShieldData = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(shieldCollisionData->PlayerShieldHitbox->ParentEntityRef);

            ReturnCode returnCode = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerShieldData->PlayerEntityRef)->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnProjectileHitPlayerShield(f, projectileCollisionData, shieldCollisionData);
        }

        /// <summary>
        /// Calls the OnUpdate method of the class of the given player character, if it is implemented.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Reference to the player handle.</param>
        /// <param name="playerData">Pointer to the player data.</param>
        /// <param name="playerEntity">Reference to the player entity.</param>
        public static void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, BattlePlayerEntityRef playerEntity)
        {
            ReturnCode returnCode = GetClass(playerData->CharacterClass, out BattlePlayerClassBase playerClass);

            if (returnCode != ReturnCode.ClassRetrieved) return;

            playerClass.OnUpdate(f, playerHandle, playerData, playerEntity);
        }

        /// <value>Constant for a class index error.</value>
        private const int ClassIndexError = -1;
        /// <value>Constant for Desensitizer class index.</value>
        private const int ClassIndex100 = 0;
        /// <value>Constant for Trickster class index.</value>
        private const int ClassIndex200 = 1;
        /// <value>Constant for Obedient class index.</value>
        private const int ClassIndex300 = 2;
        /// <value>Constant for Projector class index.</value>
        private const int ClassIndex400 = 3;
        /// <value>Constant for Retroflector class index.</value>
        private const int ClassIndex500 = 4;
        /// <value>Constant for Confluent class index.</value>
        private const int ClassIndex600 = 5;
        /// <value>Constant for Intellectualizer class index.</value>
        private const int ClassIndex700 = 6;
        /// <value>Constant for the amount of classes that exist.</value>
        private const int ClassCount = 7;

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

        /// <summary>
        /// Enum defines the state of a <see cref="BattlePlayerClassBase">BattlePlayerClass</see> in <see cref="ClassEntry"/>.
        /// </summary>
        private enum ClassState
        {
            /// <summary>Class is not loaded yet.</summary>
            NotLoaded,
            /// <summary>Class has been attempted to be loaded but has no code.</summary>
            NoCode,
            /// <summary>Class has been loaded.</summary>
            Loaded
        }

        /// <summary>
        /// Struct that holds the <see cref="BattlePlayerClassBase">BattlePlayerClass</see> script and <see cref="ClassState"/> of an entry in <see cref="s_classArray"/>
        /// </summary>
        private struct ClassEntry
        {
            /// <summary><see cref="BattlePlayerClassBase">BattlePlayerClass</see> script of a class</summary>
            public BattlePlayerClassBase Class;
            /// <summary><see cref="ClassState"/> of a class</summary>
            public ClassState State;
        }

        /// <Summary>An array containing a <see cref="ClassEntry"/> for all of the <see cref="Quantum.BattlePlayerCharacterClass">BattlePlayerCharacterClasses</see></Summary>
        private static readonly ClassEntry[] s_classArray = new ClassEntry[ClassCount];

        /// <value>A dictionary used for tracking which classes have already had an error message sent regarding their missing implementation.</value>
        private static readonly Dictionary<BattlePlayerCharacterClass, bool> s_errorMessagesSent = new();

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>
        /// Loads a class and saves it to to to @cref{s_classArray} at <paramref name="classIndex"/>
        /// </summary>
        ///
        /// <param name="characterClass">Character class</param>
        /// <param name="classIndex">Index of the character class</param>
        /// <param name="class">Normal class script for the given character class</param>
        /// <param name="testClass">Test class script for the given character class</param>
        /// <param name="testMode">Is test mode enabled</param>
        private static void LoadClass(BattlePlayerCharacterClass characterClass, int classIndex, BattlePlayerClassBase @class, BattlePlayerClassBase testClass, bool testMode)
        {
            BattlePlayerClassBase loadedClass = PickClass(@class, testClass, testMode) switch
            {
                ClassOption.UseClass     => @class,
                ClassOption.UseTestClass => testClass,

                _ => null
            };

            if (loadedClass == null)
            {
                s_classArray[classIndex].Class = null;
                s_classArray[classIndex].State = ClassState.NoCode;
                return;
            }

            if (loadedClass.Class != characterClass)
            {
                s_debugLogger.ErrorFormat("Mapping of character classes is incorrect! Expected {0}, got {1}", characterClass, loadedClass.Class);
                loadedClass = null;
            }

            s_classArray[classIndex].Class = loadedClass;
            s_classArray[classIndex].State = ClassState.Loaded;
        }

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

            if (characterClass == BattlePlayerCharacterClass.None)
            {
                return ReturnCode.NoClass;
            }

            int classIndex = characterClass switch
            {
                BattlePlayerCharacterClass.Class100 => ClassIndex100,
                BattlePlayerCharacterClass.Class200 => ClassIndex200,
                BattlePlayerCharacterClass.Class300 => ClassIndex300,
                BattlePlayerCharacterClass.Class400 => ClassIndex400,
                BattlePlayerCharacterClass.Class500 => ClassIndex500,
                BattlePlayerCharacterClass.Class600 => ClassIndex600,
                BattlePlayerCharacterClass.Class700 => ClassIndex700,

                _ => ClassIndexError,
            };

            if (classIndex == ClassIndexError)
            {
                if (!s_errorMessagesSent.ContainsKey(characterClass))
                {
                    s_errorMessagesSent.Add(characterClass, false);
                }

                if (!s_errorMessagesSent[characterClass])
                {
                    s_debugLogger.WarningFormat("The {0} class is not currently implemented!", characterClass);
                    s_errorMessagesSent[characterClass] = true;
                }

                return ReturnCode.Error;
            }

            ClassEntry classEntry = s_classArray[classIndex];

            if (classEntry.State == ClassState.NoCode)
            {
                return ReturnCode.NoClass;
            }

            if (classEntry.Class.Class != characterClass)
            {
                return ReturnCode.Error;
            }

            playerClass = classEntry.Class;
            return ReturnCode.ClassRetrieved;
        }
    }
}
