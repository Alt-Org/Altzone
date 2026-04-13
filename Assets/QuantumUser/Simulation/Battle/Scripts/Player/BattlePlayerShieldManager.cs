/// @file BattlePlayerShieldManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerShieldManager} class which handles player shield logic.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// PlayerShieldManager handles player shield management, allowing other classes to focus on gameplay logic.
    /// </summary>
    ///
    /// See [{ShieldManager}](#page-concepts-player-simulation-management-shieldmanager) for more info.
    /// See [{Player Overview}](#page-concepts-player-overview) for more info.
    /// See [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview) for more info.
    ///
    /// @anchor BattlePlayerShieldManager-ShieldGroupIndex
    /// @bigtext{**%Shield Index**}
    ///
    /// Shield Index is used internally to index shield data arrays in @cref{Quantum,BattlePlayerShieldManagerDataQSingleton}.
    /// Each [{Player Character Number}](#page-concepts-player-character-entity-character-number) per
    /// player [{Slot}](#page-concepts-player-slots-teams) is mapped to the internal Shield Index.
    public static unsafe class BattlePlayerShieldManager
    {
        #region Public Static Methods

        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create(typeof(BattlePlayerShieldManager));
        }

        /// <summary>
        /// Creates all shield entities for specified player character, initializing data, hitboxes and view components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="playerCharacterNumber">The character number of the specified character.</param>
        /// <param name="playerCharacterId">The ID of the specified character.</param>
        /// <param name="playerCharacterClass">The character class of the specified character.</param>
        /// <param name="playerCharacterEntityRef">EntityRef to the specified character's entity.</param>
        public static int CreateShields(Frame f, BattlePlayerSlot playerSlot, int playerCharacterNumber, BattlePlayerCharacterID playerCharacterId, BattlePlayerCharacterClass playerCharacterClass, BattlePlayerEntityRef playerCharacterEntityRef)
        {
            s_debugLogger.LogFormat(f, "({0}) Creating shields for character ID {1}", playerSlot, playerCharacterId);

            // get entity prototypes
            AssetRef<EntityPrototype>[] playerShieldEntityPrototypes = BattleAltzoneLink.GetShieldPrototypes(playerCharacterId);
            if (playerShieldEntityPrototypes.Length < 1)
            {
                const int FallbackId = 0;

                s_debugLogger.ErrorFormat(f, "({0}) Failed to fetch shield entity prototype array for character ID {1}\nUsing fallback ID {2}", playerSlot, playerCharacterId, FallbackId);

                playerCharacterId = FallbackId;
                playerShieldEntityPrototypes = BattleAltzoneLink.GetShieldPrototypes(playerCharacterId);

                s_debugLogger.LogFormat(f, "({0}) Creating fallback shields for character ID {1}\n", playerSlot, playerCharacterId);
            }

            BattleEntityManager.CompoundEntityTemplate[] shieldEntities = new BattleEntityManager.CompoundEntityTemplate[playerShieldEntityPrototypes.Length];

            // create shieldEntity for each shield of a character
            for (int shieldEntityIndex = 0; shieldEntityIndex < playerShieldEntityPrototypes.Length; shieldEntityIndex++)
            {
                BattlePlayerShieldEntityRef playerShieldEntityRef = BattlePlayerShieldEntityRef.Create(f, playerShieldEntityPrototypes[shieldEntityIndex]);
                BattleEntityManager.CompoundEntityTemplate playerShieldEntityTemplate = BattleEntityManager.CompoundEntityTemplate.Create(playerShieldEntityRef, playerShieldEntityPrototypes.Length);

                // get template data
                BattlePlayerShieldDataTemplateQComponent* playerShieldDataTemplate = f.Unsafe.GetPointer<BattlePlayerShieldDataTemplateQComponent>(playerShieldEntityRef);
                QList<BattlePlayerHitboxTemplate>         shieldHitboxTemplateList = f.ResolveList(playerShieldDataTemplate->HitboxList);

                //{ initialize shield component

                BattlePlayerShieldDataQComponent playerShieldData = new()
                {
                    PlayerEntityRef = playerCharacterEntityRef,
                    ShieldNumber = shieldEntityIndex
                };

                //} initialize shield component

                // create shield hitbox entities
                for (int shieldHitboxEntityIndex = 0; shieldHitboxEntityIndex < shieldHitboxTemplateList.Count; shieldHitboxEntityIndex++)
                {
                    EntityRef playerShieldHitboxEntity = f.Create();

                    // set hitbox variables
                    BattlePlayerHitboxType    playerHitboxType          = BattlePlayerHitboxType.Shield;
                    BattlePlayerCollisionType playerHitboxCollisionType = shieldHitboxTemplateList[shieldHitboxEntityIndex].CollisionType;
                    FPVector2                 playerHitboxNormal        = FPVector2.Rotate(FPVector2.Up, FP.Deg2Rad * shieldHitboxTemplateList[shieldHitboxEntityIndex].NormalAngleDeg);
                    int                       playerHitboxHeight        = 0;

                    // get hitbox template data
                    f.TryResolveList(shieldHitboxTemplateList[shieldHitboxEntityIndex].ColliderTemplateList, out QList<BattlePlayerHitboxColliderTemplate> playerShieldHitboxListColliderTemplate);

                    // initialize collisionTrigger
                    BattleCollisionTriggerQComponent collisionTrigger = BattleCollisionQSystem.CreateCollisionTriggerComponent(BattleCollisionTriggerType.Shield);

                    //{ initialize hitbox collider

                    PhysicsCollider2D playerHitboxCollider = PhysicsCollider2D.Create(f,
                        shape: Shape2D.CreatePersistentCompound(),
                        isTrigger: true
                    );

                    // create hitbox collider shape
                    foreach (BattlePlayerHitboxColliderTemplate playerShieldHitboxColliderTemplate in playerShieldHitboxListColliderTemplate)
                    {
                        playerHitboxHeight = Mathf.Max(playerShieldHitboxColliderTemplate.Position.Y, playerHitboxHeight);

                        FPVector2 playerHitboxExtents = new(
                            (FP)playerShieldHitboxColliderTemplate.Size.X * BattleGridManager.GridScaleFactor * FP._0_50,
                            (FP)playerShieldHitboxColliderTemplate.Size.Y * BattleGridManager.GridScaleFactor * FP._0_50
                        );

                        FPVector2 playerHitboxPosition = new(
                            ((FP)playerShieldHitboxColliderTemplate.Position.X - FP._0_50) * BattleGridManager.GridScaleFactor + playerHitboxExtents.X,
                            ((FP)playerShieldHitboxColliderTemplate.Position.Y + FP._0_50) * BattleGridManager.GridScaleFactor - playerHitboxExtents.Y
                        );

                        Shape2D playerHitboxColliderPart = Shape2D.CreateBox(playerHitboxExtents, playerHitboxPosition);
                        playerHitboxCollider.Shape.Compound.AddShape(f, ref playerHitboxColliderPart);
                    }

                    //} initialize hitbox collider

                    // initialize hitbox component
                    BattlePlayerHitboxQComponent playerHitbox = new()
                    {
                        ParentEntityRef    = playerShieldEntityRef,
                        HitboxType         = playerHitboxType,
                        CollisionType      = playerHitboxCollisionType,
                        CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
                    };

                    // initialize hitbox entity
                    f.Add(playerShieldHitboxEntity, playerHitbox);
                    f.Add<Transform2D>(playerShieldHitboxEntity);
                    f.Add(playerShieldHitboxEntity, playerHitboxCollider);
                    f.Add(playerShieldHitboxEntity, collisionTrigger);

                    // link hitbox
                    playerShieldEntityTemplate.Link(playerShieldHitboxEntity, new FPVector2(0, 0));
                } // create shield hitbox entities

                // initialize entity
                f.Remove<BattlePlayerShieldDataTemplateQComponent>(playerShieldEntityRef);
                f.Add(playerShieldEntityRef, playerShieldData);

                shieldEntities[shieldEntityIndex] = playerShieldEntityTemplate;

                // initialize view
                f.Events.BattlePlayerShieldViewInit(playerShieldEntityRef, playerCharacterEntityRef, playerSlot, playerCharacterId, playerCharacterClass, BattleGridManager.GridScaleFactor);
            } // create entities

            BattleEntityID shieldEntityGroupID = BattleEntityManager.RegisterCompound(f, shieldEntities);

            SetShieldEntityGroupID(f, playerSlot, playerCharacterNumber, shieldEntityGroupID);

            int playerShieldIndex = GetShieldIndex(playerSlot, playerCharacterNumber);
            GetPlayerShieldManagerData(f)->PlayerShieldCounts[playerShieldIndex] = shieldEntities.Length;

            return shieldEntities.Length;
        }

        /// <summary>
        /// Verifies that the given <paramref name="shieldNumber"/> is valid for specified player's specified character.
        /// </summary>
        ///
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The shield number to be verified.</param>
        ///
        /// <returns>True if the shield number is valid, false if it isn't.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidShieldNumber(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            if (!BattlePlayerManager.PlayerHandle.IsValidCharacterNumber(characterNumber)) return false;

            int shieldIndex = GetShieldIndex(playerSlot, characterNumber);

            if (shieldNumber < 0 || shieldNumber > GetPlayerShieldManagerData(f)->PlayerShieldCounts[shieldIndex] - 1)
            {
                s_debugLogger.ErrorFormat(f, "({0}) shield number {1} is not valid for character number {2}", playerSlot, shieldNumber, characterNumber);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attaches a shield to a player's character based on their <paramref name="playerSlot"/>, <paramref name="characterNumber"/> and <paramref name="shieldNumber"/>
        /// and updates necessary data. Additionally teleports the shield to the character if needed.
        /// </summary>
        ///
        /// See [{Attached Shield}](#page-concepts-player-character-entity-shield-attach) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.<br/>
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the desired player.</param>
        /// <param name="characterNumber">The character number of the desired character.</param>
        /// <param name="shieldNumber">The number of the shield that's being attached.</param>
        /// <param name="teleport">Whether to teleport the attached shield or not.</param>
        public static void AttachShield(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber, bool teleport = true)
        {
            if (!IsValidShieldNumber(f, playerSlot, characterNumber, shieldNumber)) return;

            BattlePlayerShieldManagerDataQSingleton* shieldManagerData = GetPlayerShieldManagerData(f);

            BattlePlayerShieldEntityRef       shieldNewEntityRef = GetShieldEntityRef(f, shieldManagerData, playerSlot, characterNumber, shieldNumber);
            BattlePlayerShieldDataQComponent* shieldNewData      = shieldNewEntityRef.GetDataQComponent(f);
            BattlePlayerEntityRef             playerEntityRef    = shieldNewData->PlayerEntityRef;
            BattlePlayerDataQComponent*       playerData         = playerEntityRef.GetDataQComponent(f);

            if (playerData->AttachedShield.ERef != EntityRef.None)
            {
                playerData->AttachedShield.GetDataQComponent(f)->IsAttached = false;

                ReturnShieldEntityRef(f, shieldManagerData, playerSlot, characterNumber, playerData->AttachedShieldNumber);
            }

            s_debugLogger.LogFormat(f, DebugMessageShieldAttachFormat, playerSlot, shieldNumber, characterNumber);
            shieldNewData->IsAttached        = true;
            playerData->AttachedShieldNumber = shieldNumber;
            playerData->AttachedShield       = shieldNewEntityRef;

            if (teleport)
            {
                FPVector2 playerPosition = playerEntityRef.GetTransform(f)->Position;
                BattleEntityManager.TeleportCompound(f, shieldNewEntityRef, playerPosition, playerData->RotationBaseRad);
            }
        }

        /// <summary>
        /// Retrieves a detached shield linked to a character based on their <paramref name="characterNumber"/> and <paramref name="shieldNumber"/>
        /// and updates necessary data.
        /// </summary>
        ///
        /// See [{Detached Shield}](#page-concepts-player-character-entity-shield-attach) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.<br/>
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified character.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The number of the shield that's being retrieved.</param>
        ///
        /// <returns>The BattlePlayerShieldEntityRef of the detached shield.</returns>
        public static BattlePlayerShieldEntityRef GetDetachedShieldEntityRef(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            if (!IsValidShieldNumber(f, playerSlot, characterNumber, shieldNumber)) return BattlePlayerShieldEntityRef.None;

            BattlePlayerShieldManagerDataQSingleton* shieldManagerData = GetPlayerShieldManagerData(f);

            BattlePlayerShieldEntityRef       shieldEntityRef = GetShieldEntityRef(f, shieldManagerData, playerSlot, characterNumber, shieldNumber);
            BattlePlayerShieldDataQComponent* shieldData      = shieldEntityRef.GetDataQComponent(f);

            if (!shieldData->IsAttached) return shieldEntityRef;

            s_debugLogger.LogFormat(f, DebugMessageShieldDetachFormat, playerSlot, shieldNumber, characterNumber);

            BattlePlayerDataQComponent* playerData = shieldData->PlayerEntityRef.GetDataQComponent(f);

            playerData->AttachedShield = BattlePlayerShieldEntityRef.None;
            shieldData->IsAttached     = false;

            return shieldEntityRef;
        }

        /// <summary>
        /// Removes and detaches a shield attached to a character based on their <paramref name="characterNumber"/> and <paramref name="shieldNumber"/>
        /// and updates necessary data.
        /// </summary>
        ///
        /// See [{Detached Shield}](#page-concepts-player-character-entity-shield-attach) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.<br/>
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified character.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The number of the shield that's being removed.</param>
        public static void RemoveShield(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            if (!IsValidShieldNumber(f, playerSlot, characterNumber, shieldNumber)) return;

            BattlePlayerShieldManagerDataQSingleton* shieldManagerData = GetPlayerShieldManagerData(f);

            BattlePlayerShieldEntityRef       shieldEntityRef = GetShieldEntityRef(f, shieldManagerData, playerSlot, characterNumber, shieldNumber);
            BattlePlayerShieldDataQComponent* shieldData      = shieldEntityRef.GetDataQComponent(f);
            BattlePlayerDataQComponent*       playerData      = shieldData->PlayerEntityRef.GetDataQComponent(f);

            if (playerData->AttachedShield.ERef == shieldEntityRef)
            {
                s_debugLogger.LogFormat(f, DebugMessageShieldDetachFormat, playerSlot, shieldNumber, characterNumber);
                playerData->AttachedShieldNumber = -1;
                playerData->AttachedShield       = BattlePlayerShieldEntityRef.None;
            }

            s_debugLogger.LogFormat(f, DebugMessageShieldRemovedFormat, playerSlot, shieldNumber, characterNumber);

            shieldData->IsAttached = false;

            ReturnShieldEntityRef(f, shieldManagerData, playerSlot, characterNumber, playerData->AttachedShieldNumber);
        }

        /// <summary>
        /// Low level method for retrieving the BattleEntityID of the specified shield of the specified player character.
        /// @note Low level method! Only meant for use by <see cref="BattlePlayerManager"/>.
        /// </summary>
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified character.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        ///
        /// <returns>The BattleEntityID of the specified shield.</returns>
        public static BattleEntityID Low_GetShieldEntityGroupID(Frame f, BattlePlayerSlot playerSlot, int characterNumber)
        {
            return GetPlayerShieldManagerData(f)->PlayerShieldEntityGroupIDs[GetShieldIndex(playerSlot, characterNumber)];
        }

        #endregion Public Static Methods

        /// @anchor BattlePlayerShieldManager-PrivateDebugMessageConstants
        /// @name Constant Private DebugMessage Attributes
        /// Constants defining debug messages
        /// @{
        #region Private DebugMessage Constants

        /// <summary>Formatted debug message for when a shield is attached.</summary>
        private const string DebugMessageShieldAttachFormat = "({0}) Attaching shield number {1} to character {2}";

        /// <summary>Formatted debug message for when a shield is detached.</summary>
        private const string DebugMessageShieldDetachFormat = "({0}) Deattaching shield number {1} from character {2}";

        /// <summary>Formatted debug message for when a shield is removed.</summary>
        private const string DebugMessageShieldRemovedFormat = "({0}) Removing shield number {1} of character {2}";

        #endregion Private DebugMessage Constants
        /// @}

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        #region Private Static Methods

        /// <summary>
        /// Private helper method for getting the BattlePlayerShieldManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// See [{ShieldManagerData}](#page-concepts-player-simulation-management-shieldmanagerdata) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer to the PlayerShieldManagerData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerShieldManagerDataQSingleton* GetPlayerShieldManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerShieldManagerDataQSingleton* playerShieldManagerData))
            {
                BattleDebugLogger.Error(f, nameof(BattlePlayerShieldManager), "PlayerShieldManagerData singleton not found!");
            }

            return playerShieldManagerData;
        }

        /// <summary>
        /// Private helper method for retrieving the index in BattlePlayerShieldManagerDataQSingleton of the shield entity group for a specified player character.
        /// </summary>
        ///
        /// See @ref BattlePlayerShieldManager-ShieldGroupIndex "ShieldIndex" for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="playerSlot">The BattlePlayerSlot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        ///
        /// <returns>The index in BattlePlayerShieldManagerDataQSingleton for the specified shield.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetShieldIndex(BattlePlayerSlot playerSlot, int characterNumber)
        {
            int playerIndex = BattlePlayerManager.PlayerHandle.Low_GetPlayerIndex(playerSlot);
            return playerIndex * Constants.BATTLE_PLAYER_CHARACTER_COUNT + characterNumber;
        }

        /// @anchor BattlePlayerShieldManager-PrivateStaticMethods-ShieldEntity
        /// @name Shield Entity Methods
        /// Methods for handling shield entities
        /// @{
        #region Private Static Methods - Shield Entity

        /// <summary>
        /// Sets the shield entity IDs for specified player's specified character in BattlePlayerShieldManagerDataQSingleton.
        /// </summary>
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldEntityGroupID">The entity group's ID to be set.</param>
        private static void SetShieldEntityGroupID(Frame f, BattlePlayerSlot playerSlot, int characterNumber, BattleEntityID shieldEntityGroupID)
        {
            BattlePlayerShieldManagerDataQSingleton* playerShieldManagerSingleton = GetPlayerShieldManagerData(f);
            playerShieldManagerSingleton->PlayerShieldEntityGroupIDs[GetShieldIndex(playerSlot, characterNumber)] = shieldEntityGroupID;
        }

        /// <summary>
        /// Private helper method for retrieving a shield entity based on given <paramref name="shieldNumber"/>
        /// for specified <paramref name="playerSlot"/>'s specified <paramref name="characterNumber"/>.
        /// </summary>
        ///
        /// See [{Shield Entity}](#page-concepts-player-character-and-shield-entity) for more info.<br/>
        /// See [{ShieldManagerData}](#page-concepts-player-simulation-management-shieldmanagerdata) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.<br/>
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.<br/>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="shieldManagerData">Pointer to shield manager data.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The shield number of the shield to be retrieved.</param>
        ///
        /// <returns>EntityRef of the retrieved shield entity.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerShieldEntityRef GetShieldEntityRef(Frame f, BattlePlayerShieldManagerDataQSingleton* shieldManagerData, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            BattleEntityID shieldGroupID = shieldManagerData->PlayerShieldEntityGroupIDs[GetShieldIndex(playerSlot, characterNumber)];
            return (BattlePlayerShieldEntityRef)BattleEntityManager.Get(f, shieldGroupID, shieldNumber);
        }

        /// <summary>
        /// Private helper method for returning a shield entity based on given <paramref name="shieldNumber"/>
        /// for specified <paramref name="playerSlot"/>'s specified <paramref name="characterNumber"/>.
        /// </summary>
        ///
        /// See [{Shield Entity}](#page-concepts-player-character-and-shield-entity) for more info.<br/>
        /// See [{ShieldManagerData}](#page-concepts-player-simulation-management-shieldmanagerdata) for more info.<br/>
        /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.<br/>
        /// See [{Shield Number}](#page-concepts-player-character-entity-shield-number) for more info.<br/>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="shieldManagerData">Pointer to shield manager data.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The shield number of the shield to be returned.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnShieldEntityRef(Frame f, BattlePlayerShieldManagerDataQSingleton* shieldManagerData, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            BattleEntityID shieldGroupID = shieldManagerData->PlayerShieldEntityGroupIDs[GetShieldIndex(playerSlot, characterNumber)];
            BattleEntityManager.Return(f, shieldGroupID, shieldNumber);
        }

        #endregion Private Static Methods - Shield Entity
        /// @}

        #endregion Private Static Methods
    }
}
