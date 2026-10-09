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
        public static int CreateShields(Frame f, BattlePlayerData.Ref playerData, int playerCharacterNumber, BattlePlayerCharacterID playerCharacterId, BattlePlayerCharacterClass playerCharacterClass, BattlePlayerEntityRef playerCharacterEntityRef)
        {
            s_debugLogger.LogFormat(f, "({0}) Creating shields for character ID {1}", playerData.Slot, playerCharacterId);

            // get entity prototypes
            AssetRef<EntityPrototype>[] playerShieldEntityPrototypes = BattleAltzoneLink.GetShieldPrototypes(playerCharacterId);
            if (playerShieldEntityPrototypes.Length < 1)
            {
                const int FallbackId = 0;

                s_debugLogger.ErrorFormat(f, "({0}) Failed to fetch shield entity prototype array for character ID {1}\nUsing fallback ID {2}", playerData.Slot, playerCharacterId, FallbackId);

                playerCharacterId = FallbackId;
                playerShieldEntityPrototypes = BattleAltzoneLink.GetShieldPrototypes(playerCharacterId);

                s_debugLogger.LogFormat(f, "({0}) Creating fallback shields for character ID {1}\n", playerData.Slot, playerCharacterId);
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
                    BattlePlayerHitboxQComponent playerShieldHitbox = new()
                    {
                        ParentEntityRef    = playerShieldEntityRef,
                        HitboxType         = playerHitboxType,
                        CollisionType      = playerHitboxCollisionType,
                        CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
                    };

                    // initialize hitbox entity
                    f.Add(playerShieldHitboxEntity, playerShieldHitbox);
                    f.Add<Transform2D>(playerShieldHitboxEntity);
                    f.Add(playerShieldHitboxEntity, playerHitboxCollider);
                    f.Add(playerShieldHitboxEntity, collisionTrigger);

                    // link hitbox
                    playerShieldEntityTemplate.Link(playerShieldHitboxEntity, new FPVector2(0, 0));
                    BattlePlayerCharacterShieldHandle.CreateLink(f, playerShieldEntityRef, PlayerCharacterShieldEntityType.Hitbox, playerShieldHitboxEntity);
                    BattlePlayerHandle.CreateLink(f, playerData.Slot, playerCharacterEntityRef, PlayerEntityType.Shield, playerShieldHitboxEntity);
                } // create shield hitbox entities

                // initialize entity
                f.Remove<BattlePlayerShieldDataTemplateQComponent>(playerShieldEntityRef);
                f.Add(playerShieldEntityRef, playerShieldData);

                BattlePlayerCharacterShieldHandle.CreateLink(f, playerShieldEntityRef, PlayerCharacterShieldEntityType.Shield, playerShieldEntityRef);
                BattlePlayerHandle.CreateLink(f, playerData.Slot, playerCharacterEntityRef, PlayerEntityType.Shield, playerShieldEntityRef);

                shieldEntities[shieldEntityIndex] = playerShieldEntityTemplate;

                // initialize view
                f.Events.BattlePlayerShieldViewInit(playerShieldEntityRef, playerCharacterEntityRef, playerData.Slot, playerCharacterId, playerCharacterClass, shieldEntityIndex, BattleGridManager.GridScaleFactor);
            } // create entities

            BattleEntityID shieldEntityGroupID = BattleEntityManager.RegisterCompound(f, shieldEntities);

            playerData.Low_Level.ShieldEntityGroupIDs[playerCharacterNumber] = shieldEntityGroupID;

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
            /*if (!BattlePlayerData.IsValidCharacterNumber(characterNumber)) return false;

            int shieldIndex = GetShieldIndex(playerSlot, characterNumber);

            if (shieldNumber < 0 || shieldNumber > GetPlayerShieldManagerData(f)->PlayerShieldCounts[shieldIndex] - 1)
            {
                s_debugLogger.ErrorFormat(f, "({0}) shield number {1} is not valid for character number {2}", playerSlot, shieldNumber, characterNumber);
                return false;
            }*/

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
        public static void AttachShield(Frame f, BattlePlayerHandle playerHandle, int characterNumber, int shieldNumber, bool teleport = true)
        {
            if (!IsValidShieldNumber(f, playerHandle.PlayerData.Slot, characterNumber, shieldNumber)) return;

            BattlePlayerCharacterShieldHandle shieldNewHandle = GetShield(f, playerHandle.PlayerData, characterNumber, shieldNumber, updateViewPlayState: teleport);

            if (playerHandle.LoadedCharacterHasShieldAttached)
            {
                BattlePlayerCharacterShieldHandle shieldHandle = playerHandle.GetLoadedCharacterAttachedShield(f);

                ReturnShieldEntityRef(f, playerHandle.PlayerData, characterNumber, shieldHandle.ShieldData->ShieldNumber);
            }

            s_debugLogger.LogFormat(f, DebugMessageShieldAttachFormat, playerHandle.PlayerData.Slot, shieldNumber, characterNumber);
            shieldNewHandle.ShieldData->IsAttached                    = true;
            playerHandle.LoadedCharacterData->AttachedShieldEntityRef = shieldNewHandle.ShieldEntityRef;

            if (teleport)
            {
                FPVector2 playerPosition = playerHandle.LoadedCharacterTransform->Position;
                BattleEntityManager.TeleportCompound(f, shieldNewHandle.ShieldEntityRef, playerPosition, playerHandle.LoadedCharacterData->MovementRotationBaseRad);
            }
            f.Events.BattleShieldChangeState(playerHandle.LoadedCharacterEntityRef, playerHandle.PlayerData.Team, shieldNewHandle.ShieldData->IsAttached, shieldNumber);
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
        /// <param name="updateViewPlayState">Whether to update the view play state or not.</param>
        ///
        /// <returns>The BattlePlayerShieldEntityRef of the detached shield.</returns>
        public static BattlePlayerShieldEntityRef GetDetachedShieldEntityRef(Frame f, BattlePlayerHandle playerHandle, int characterNumber, int shieldNumber, bool updateViewPlayState = false)
        {
            if (!IsValidShieldNumber(f, playerHandle.PlayerData.Slot, characterNumber, shieldNumber)) return BattlePlayerShieldEntityRef.None;

            BattlePlayerCharacterShieldHandle shieldHandle = GetShield(f, playerHandle.PlayerData, characterNumber, shieldNumber, updateViewPlayState);

            if (!shieldHandle.ShieldData->IsAttached) return (BattlePlayerShieldEntityRef)shieldHandle.ShieldEntityRef;

            s_debugLogger.LogFormat(f, DebugMessageShieldDetachFormat, playerHandle.PlayerData.Slot, shieldNumber, characterNumber);

            playerHandle.LoadedCharacterData->AttachedShieldEntityRef = BattlePlayerShieldEntityRef.None;
            shieldHandle.ShieldData->IsAttached                       = false;

            f.Events.BattleShieldChangeState(shieldHandle.ShieldData->PlayerEntityRef.ERef, playerHandle.PlayerData.Team, shieldHandle.ShieldData->IsAttached, shieldNumber);
            return (BattlePlayerShieldEntityRef)shieldHandle.ShieldEntityRef;
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
        public static void RemoveShield(Frame f, BattlePlayerHandle playerHandle, int characterNumber, int shieldNumber)
        {
            if (!IsValidShieldNumber(f, playerHandle.PlayerData.Slot, characterNumber, shieldNumber)) return;

            BattlePlayerCharacterShieldHandle shieldHandle = GetShield(f, playerHandle.PlayerData, characterNumber, shieldNumber, updateViewPlayState: false);

            if (playerHandle.LoadedCharacterData->AttachedShieldEntityRef == (BattlePlayerShieldEntityRef)shieldHandle.ShieldEntityRef)
            {
                s_debugLogger.LogFormat(f, DebugMessageShieldDetachFormat, playerHandle.PlayerData.Slot, shieldNumber, characterNumber);
                playerHandle.LoadedCharacterData->AttachedShieldEntityRef = BattlePlayerShieldEntityRef.None;
            }

            s_debugLogger.LogFormat(f, DebugMessageShieldRemovedFormat, playerHandle.PlayerData.Slot, shieldNumber, characterNumber);

            shieldHandle.ShieldData->IsAttached = false;

            ReturnShieldEntityRef(f, playerHandle.PlayerData, characterNumber, shieldNumber);
            f.Events.BattleShieldChangeState(playerHandle.LoadedCharacterEntityRef, playerHandle.PlayerData.Team, shieldHandle.ShieldData->IsAttached, shieldNumber);
        }

        public static bool IsValidShieldNumber(BattlePlayerHandle playerHandle, int shieldNumber)
        {
            return playerHandle.IsValidShieldNumber(playerHandle.LoadedCharacterData->Number, shieldNumber);
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

        /// @anchor BattlePlayerShieldManager-PrivateStaticMethods-ShieldEntity
        /// @name Shield Entity Methods
        /// Methods for handling shield entities
        /// @{
        #region Private Static Methods - Shield Entity

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
        /// <param name="updateViewPlayState">Whether to update the view play state or not.</param>
        ///
        /// <returns>EntityRef of the retrieved shield entity.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerCharacterShieldHandle GetShield(Frame f, BattlePlayerData.Ref playerData, int characterNumber, int shieldNumber, bool updateViewPlayState)
        {
            BattleEntityID shieldGroupID = playerData.Low_Level.ShieldEntityGroupIDs[characterNumber];
            return BattlePlayerCharacterShieldHandle.Create(f, BattleEntityManager.Get(f, shieldGroupID, shieldNumber, updateViewPlayState));
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
        private static void ReturnShieldEntityRef(Frame f, BattlePlayerData.Ref playerData, int characterNumber, int shieldNumber)
        {
            BattleEntityID shieldGroupID = playerData.Low_Level.ShieldEntityGroupIDs[characterNumber];
            BattleEntityManager.Return(f, shieldGroupID, shieldNumber);
        }

        #endregion Private Static Methods - Shield Entity
        /// @}

        #endregion Private Static Methods
    }
}
