/// @file BattlePlayerShieldManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerShieldManager} class which handles player shield logic.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Photon.Deterministic;

// Quantum usings
using Quantum;
using Quantum.Collections;

namespace Quantum
{
    public unsafe partial struct BattlePlayerShieldEntityRef
    {
        public static BattlePlayerShieldEntityRef None => default;

        public static implicit operator EntityRef(BattlePlayerShieldEntityRef battlePlayerShieldEntityRef) => battlePlayerShieldEntityRef.ERef;

        public static explicit operator BattlePlayerShieldEntityRef(EntityRef entityRef) => new BattlePlayerShieldEntityRef() { ERef = entityRef };

        public Transform2D* GetTransform(Frame f) => f.Unsafe.GetPointer<Transform2D>(ERef);
        public BattlePlayerShieldDataQComponent* GetDataQComponent(Frame f) => f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(ERef);
    }
}

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// PlayerShieldManager handles player shield management, allowing other classes to focus on gameplay logic.<br/>
    /// Provides static methods to create, retrieve, and despawn player shields.
    /// </summary>
    ///
    /// Handles creating shield entities for player characters, as well as retrieving and despawning them.<br/>
    public static unsafe class BattlePlayerShieldManager
    {
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
        /// <param name="playerCharacterEntity">EntityRef to the speficied character's entity.</param>
        public static int CreateShields(Frame f, BattlePlayerSlot playerSlot, int playerCharacterNumber, int playerCharacterId, BattlePlayerEntityRef playerCharacterEntity)
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

            EntityRef[] shieldEntities = new EntityRef[playerShieldEntityPrototypes.Length];

            // create entities
            for (int shieldEntityIndex = 0; shieldEntityIndex < playerShieldEntityPrototypes.Length; shieldEntityIndex++)
            {
                EntityRef playerShieldEntity = f.Create(playerShieldEntityPrototypes[shieldEntityIndex]);

                // get template data
                BattlePlayerShieldDataTemplateQComponent* playerShieldDataTemplate = f.Unsafe.GetPointer<BattlePlayerShieldDataTemplateQComponent>(playerShieldEntity);
                QList<BattlePlayerHitboxTemplate>         shieldHitboxTemplateList = f.ResolveList(playerShieldDataTemplate->HitboxList);

                //{ initialize shield component

                BattlePlayerShieldDataQComponent playerShieldData = new()
                {
                    PlayerEntity   = playerCharacterEntity,
                    HitboxEntities = f.AllocateList<EntityRef>(),
                    IsActive       = true
                };

                QList<EntityRef> playerShieldHitboxEntities = f.ResolveList(playerShieldData.HitboxEntities);

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
                        ParentEntity       = playerShieldEntity,
                        HitboxType         = playerHitboxType,
                        CollisionType      = playerHitboxCollisionType,
                        Normal             = playerHitboxNormal,
                        NormalBase         = playerHitboxNormal,
                        CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
                    };

                    // initialize hitbox entity
                    f.Add(playerShieldHitboxEntity, playerHitbox);
                    f.Add<Transform2D>(playerShieldHitboxEntity);
                    f.Add(playerShieldHitboxEntity, playerHitboxCollider);
                    f.Add(playerShieldHitboxEntity, collisionTrigger);

                    playerShieldHitboxEntities.Add(playerShieldHitboxEntity);
                } // create shield hitbox entities

                // initialize entity
                f.Remove<BattlePlayerShieldDataTemplateQComponent>(playerShieldEntity);
                f.Add(playerShieldEntity, playerShieldData);

                // add entity to array
                shieldEntities[shieldEntityIndex] = playerShieldEntity;

                // initialize view
                f.Events.BattleShieldViewInit(playerShieldEntity, playerSlot, BattleGridManager.GridScaleFactor);
            } // create entities

            BattleEntityID shieldEntityGroupID = BattleEntityManager.Register(f, shieldEntities);

            if (shieldEntities.Length > Constants.BATTLE_PLAYER_SHIELD_COUNT)
            {
                s_debugLogger.ErrorFormat(f, "({0}) Character number {1} has too many shield entities!\n" +
                                          "Has: {2}\n" +
                                          "Max allowed: {3}",
                                          playerSlot,
                                          playerCharacterNumber,
                                          shieldEntities.Length,
                                          Constants.BATTLE_PLAYER_SHIELD_COUNT
                );
            }

            SetShieldEntityGroupID(f, playerSlot, playerCharacterNumber, shieldEntityGroupID);

            int playerCharacterIndex = BattlePlayerManager.PlayerHandle.Low_GetPlayerIndex(playerSlot) * Constants.BATTLE_PLAYER_CHARACTER_COUNT + playerCharacterNumber;
            GetPlayerShieldManagerData(f)->PlayerShieldCounts[playerCharacterIndex] = shieldEntities.Length;

            return shieldEntities.Length;
        }

        /// <summary>
        /// Retrieves a shield entity based on given <paramref name="shieldNumber"/> for specified player's specified character.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The shield number of the shield to be retrieved.</param>
        ///
        /// <returns>EntityRef of the retrieved shield entity.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityRef GetShieldEntity(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            if (!IsValidShieldNumber(f, playerSlot, characterNumber, shieldNumber)) return EntityRef.None;

            return BattleEntityManager.Get(f, GetPlayerShieldManagerData(f)->PlayerShieldEntityGroupIDs[GetShieldGroupIndex(playerSlot, characterNumber)], shieldNumber);
        }

        /// <summary>
        /// Despawns the given shield entity based on given <paramref name="shieldNumber"/> for specified player's specified character.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldNumber">The shield number of the shield to be despawned</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DespawnShield(Frame f, BattlePlayerSlot playerSlot, int characterNumber, int shieldNumber)
        {
            if (!IsValidShieldNumber(f, playerSlot, characterNumber, shieldNumber)) return;

            BattleEntityManager.Return(f, GetPlayerShieldManagerData(f)->PlayerShieldEntityGroupIDs[GetShieldGroupIndex(playerSlot, characterNumber)], shieldNumber);
        }

        /// <summary>
        /// Verifies that the given <paramref name="shieldNumber"/> is valid for specified player's specified character.
        /// </summary>
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

            int characterIndex = BattlePlayerManager.PlayerHandle.Low_GetPlayerIndex(playerSlot) * Constants.BATTLE_PLAYER_CHARACTER_COUNT + characterNumber;

            if (shieldNumber < 0 || shieldNumber > GetPlayerShieldManagerData(f)->PlayerShieldCounts[characterIndex] - 1)
            {
                s_debugLogger.ErrorFormat(f, "({0}) shield number {1} is not valid for character number {2}", playerSlot, shieldNumber, characterNumber);
                return false;
            }

            return true;
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>
        /// Sets the shield entity IDs for specified player's specified character in BattlePlayerShieldManagerDataQSingleton.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerSlot">Slot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        /// <param name="shieldEntityGroupID">The entity group's ID to be set.</param>
        private static void SetShieldEntityGroupID(Frame f, BattlePlayerSlot playerSlot, int characterNumber, BattleEntityID shieldEntityGroupID)
        {
            BattlePlayerShieldManagerDataQSingleton* playerShieldManagerSingleton = GetPlayerShieldManagerData(f);

            playerShieldManagerSingleton->PlayerShieldEntityGroupIDs[GetShieldGroupIndex(playerSlot, characterNumber)] = shieldEntityGroupID;
        }

        /// <summary>
        /// Retrieves the index in BattlePlayerShieldManagerDataQSingleton of the shield entity group for a specified player character.
        /// </summary>
        ///
        /// <param name="playerSlot">The BattlePlayerSlot of the specified player.</param>
        /// <param name="characterNumber">The character number of the specified character.</param>
        ///
        /// <returns>The index in BattlePlayerShieldManagerDataQSingleton for the specified shield.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetShieldGroupIndex(BattlePlayerSlot playerSlot, int characterNumber)
        {
            int playerIndex = BattlePlayerManager.PlayerHandle.Low_GetPlayerIndex(playerSlot);

            return playerIndex * Constants.BATTLE_PLAYER_CHARACTER_COUNT + characterNumber;
        }

        /// <summary>
        /// Private helper method for getting the BattlePlayerShieldManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer reference to the PlayerShieldManagerData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerShieldManagerDataQSingleton* GetPlayerShieldManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerShieldManagerDataQSingleton* playerShieldManagerData))
            {
                BattleDebugLogger.Error(f, nameof(BattlePlayerShieldManager), "PlayerShieldManagerData singleton not found!");
            }

            return playerShieldManagerData;
        }
    }
}
