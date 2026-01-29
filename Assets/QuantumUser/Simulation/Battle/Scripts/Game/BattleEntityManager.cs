/// @file BattleEntityManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleEntityManager} class which handles %Quantum entity management in the game.<br/>
/// Entities are registered, and can be retrieved and returned.
/// </summary>

// System usings
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

namespace Quantum
{
    // Adds functionality to convert between BattleEntityID and int
    // Main struct documentation in qtn-BattleEntityID.dox
    public partial struct BattleEntityID
    {
        /// <summary>
        /// Implicit cast to <c>int</c> implementation.
        /// </summary>
        ///
        /// <param name="id">The BattleEntityID to cast to <c>int</c>.</param>
        public static implicit operator int(BattleEntityID id) => id.Int;
    }
}

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// BattleEntityManager handles %Quantum entity management in the game.<br/>
    /// Entities are registered, and can be retrieved and returned.
    /// </summary>
    ///
    /// [{Entity Manager}](#page-concepts-entity-management-entity-manager)
    ///
    /// Stores entities offscreen when not in use. Entities are assigned a @cref{Quantum.BattleEntityID} for accessing them.<br/>
    /// Entities are retrieved using @cref{BattleEntityManager,Get} to be used in the game.<br/>
    /// Entities can be returned using @cref{BattleEntityManager,Return}, teleporting them back offscreen.
    public static unsafe class BattleEntityManager
    {
        /// <summary>
        /// Public struct used as template when handling compounds.
        /// </summary>
        public struct CompoundEntityTemplate
        {
            /// <summary>Parent entity reference of the compound.</summary>
            public EntityRef ParentEntityRef;

            /// <summary></summary>
            public List<BattleEntityLink> LinkedEntities;

            /// <summary>
            /// Public static create method to create compound.
            /// </summary>
            ///
            /// <param name="parent">Parent entity reference of the compound.</param>
            ///
            /// <returns>Returns new CompoundEntityTemplate.</returns>
            public static CompoundEntityTemplate Create(EntityRef parent)
            {
                return new CompoundEntityTemplate
                {
                    ParentEntityRef = parent,
                    LinkedEntities = new List<BattleEntityLink>()
                };
            }

            /// <summary>
            /// Public static create method to create compound.
            /// </summary>
            ///
            /// <param name="parent">Parent entity reference of the compound.</param>
            /// <param name="capacity">Capacity of the linked entities list</param>
            ///
            /// <returns>Returns new CompoundEntityTemplate.</returns>
            public static CompoundEntityTemplate Create(EntityRef parent, int capacity)
            {
                return new CompoundEntityTemplate
                {
                    ParentEntityRef = parent,
                    LinkedEntities = new List<BattleEntityLink>(capacity)
                };
            }

            /// <summary>
            /// Public method for linking entities in compound.
            /// </summary>
            ///
            /// <param name="entityRef"></param>
            /// <param name="offset"></param>
            public void Link(EntityRef entityRef, FPVector2 offset)
            {
                LinkedEntities.Add(new BattleEntityLink
                {
                    ERef = entityRef,
                    Offset = offset
                });
            }
        }

        // Entities are stored offscreen stacked vertically, starting from the set entityOffscreenPositionOffset and going down.

        /// <summary>
        /// Initializes <see cref="Quantum.BattleEntityManagerDataQSingleton">BattleEntityManagerDataQSingleton's</see>
        /// <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>,
        /// <see cref="Quantum.BattleEntityManagerDataQSingleton.EntityOffscreenPositionOffset">EntityOffscreenPositionOffset</see>
        /// and <see cref="Quantum.BattleEntityManagerDataQSingleton.EntitySpacing">EntitySpacing</see> fields.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityOffscreenPositionOffset">The offscreen BattleGridPosition of the first registered entity.</param>
        /// <param name="entitySpacing">Amount of grid spaces between entity offscreen positions.</param>
        public static void Init(Frame f, BattleGridPosition entityOffscreenPositionOffset, int entitySpacing)
        {
            s_battleDebugLogger = BattleDebugLogger.Create(typeof(BattleEntityManager));

            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);

            entityManagerData->RegisteredEntities = f.AllocateList<EntityRef>();

            entityManagerData->EntityOffscreenPositionOffset = entityOffscreenPositionOffset;
            entityManagerData->EntitySpacing = entitySpacing;
        }

        /// <summary>
        /// Registers given <paramref name="entityRef"/> to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        ///
        /// Use @cref{Register(Frame\, EntityRef[])} to register a group of entities.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">Entity to register.</param>
        ///
        /// <returns>Entity ID of the registered entity.</returns>
        public static BattleEntityID Register(Frame f, EntityRef entityRef)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count };
            entityList.Add(entityRef);
            Return(f, entityManagerData, entityRef, id);

            return id;
        }

        /// <summary>
        /// Registers given group of <paramref name="entityRefs"/> to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        ///
        /// Use @cref{Register(Frame\, EntityRef)} to register a single entity.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRefs">Array of entities to register.</param>
        ///
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID Register(Frame f, EntityRef[] entityRefs)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count };
            BattleEntityID offsetId = id;

            for (int i = 0; i < entityRefs.Length; i++)
            {
                entityList.Add(entityRefs[i]);
                Return(f, entityManagerData, entityRefs[i], offsetId);
                offsetId.Int++;
            }

            return id;
        }

        /// <summary>
        /// Public method for registering compound entities by using a template.
        /// </summary>
        ///
        /// Calling @clink{MakeCompound:BattleEntityManager.MakeCompound(Frame, CompoundEntityTemplate)} method to make compounds.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="template">Compound entity template used to make compounds</param>
        ///
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID RegisterCompound(Frame f, CompoundEntityTemplate template)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count, IsCompound = true };
            BattleEntityID offsetId = id;

            MakeCompound(f, template);

            EntityRef entityRef = template.ParentEntityRef;
            entityList.Add(entityRef);
            Return(f, entityManagerData, entityRef, offsetId);
            offsetId.Int++;

            return id;
        }

        /// <summary>
        /// Public method for registering compound entities by using an array of templates.
        /// </summary>
        ///
        /// Calling @clink{MakeCompound:BattleEntityManager.MakeCompound(Frame, CompoundEntityTemplate)} method to make compounds.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="templates">Compound entity templates used to make compounds</param>
        ///
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID RegisterCompound(Frame f, CompoundEntityTemplate[] templates)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count, IsCompound = true };
            BattleEntityID offsetId = id;

            for (int i = 0; i < templates.Length; i++)
            {
                CompoundEntityTemplate template = templates[i];

                MakeCompound(f, template);

                EntityRef entityRef = template.ParentEntityRef;
                entityList.Add(entityRef);
                Return(f, entityManagerData, entityRef, offsetId);
                offsetId.Int++;
            }

            return id;
        }

        /// <summary>
        /// Retrieves entity matching given <paramref name="id"/>.
        /// </summary>
        ///
        /// Use @cref{Get(Frame\, BattleEntityID\, int)} to retreive an entity in a group.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to retrieve.</param>
        ///
        /// <returns>EntityRef for retrieved entity.</returns>
        public static EntityRef Get(Frame f, BattleEntityID id)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            if (_updatePlayState)
            {
                f.Events.BattleInPlayStateUpdate(true);
            }

            return entityList[id];
        }

        /// <summary>
        /// Retrieves entity matching given <paramref name="id"/> from the group at <paramref name="offset"/>.
        /// </summary>
        ///
        /// Use @cref{Get(Frame\, BattleEntityID)} to retreive an entity not in any group.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to retrieve.</param>
        /// <param name="offset">Offset of the desired entity within the group.</param>
        ///
        /// <returns>EntityRef for retrieved entity.</returns>
        public static EntityRef Get(Frame f, BattleEntityID id, int offset)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            if (_updatePlayState)
            {
                f.Events.BattleInPlayStateUpdate(true);
            }

            return entityList[id + offset];
        }

        /// <summary>
        /// Returns entity matching given <paramref name="id"/> back offscreen.
        /// </summary>
        ///
        /// Use @cref{Get(Frame\, BattleEntityID\, int)} to return an entity in a group.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        ///
        /// Return logic handled by private @clink{Return:BattleEntityManager.Return(Frame, BattleEntityManagerDataQSingleton*, EntityRef, BattleEntityID)} method.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to return.</param>
        public static void Return(Frame f, BattleEntityID id)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            EntityRef entity = f.ResolveList(entityManagerData->RegisteredEntities)[id];

            Return(f, entityManagerData, entity, id);
        }

        /// <summary>
        /// Returns entity matching given <paramref name="id"/> from the group at <paramref name="offset"/> back offscreen.
        /// </summary>
        ///
        /// Use @cref{Return(Frame\, BattleEntityID)} to return an entity not in any group.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.
        ///
        /// Return logic handled by private @clink{Return:BattleEntityManager.Return(Frame, BattleEntityManagerDataQSingleton*, EntityRef, BattleEntityID)} method.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to return.</param>
        /// <param name="offset">Offset of the desired entity within the group.</param>
        public static void Return(Frame f, BattleEntityID id, int offset)
        {
            id.Int += offset;

            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            EntityRef entity = f.ResolveList(entityManagerData->RegisteredEntities)[id];

            Return(f, entityManagerData, entity, id);
        }

        private static bool _updatePlayState;

        /// <summary>
        /// Private method that handles entity return logic. Used by the public
        /// <see cref="BattleEntityManager.Return(Frame, BattleEntityID)">Return(Frame, BattleEntityID)</see>
        /// and <see cref="BattleEntityManager.Return(Frame, BattleEntityID, int)">Return(Frame, BattleEntityID, int)</see> methods.
        /// </summary>
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        ///
        /// Also used by the @cref{Register(Frame\, EntityRef)} and @cref{Register(Frame\, EntityRef[])} methods.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityManagerData">Pointer reference to BattleEntityManagerData singleton.</param>
        /// <param name="entity">The entity to return.</param>
        /// <param name="id">Entity ID of the entity to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Return(Frame f, BattleEntityManagerDataQSingleton* entityManagerData, EntityRef entity, BattleEntityID id)
        {
            BattleGridPosition entityGridPosition = entityManagerData->EntityOffscreenPositionOffset;
            entityGridPosition.Row -= id * entityManagerData->EntitySpacing;

            FPVector2 entityGridToWorldPosition = BattleGridManager.GridPositionToWorldPosition(entityGridPosition);

            if (id.IsCompound)
            {
                TeleportCompound(f, entity, entityGridToWorldPosition, 0);
            }
            else
            {
                f.Unsafe.GetPointer<Transform2D>(entity)->Teleport(f, entityGridToWorldPosition);
            }
        }

        /// <summary>
        /// Public method for moving compound.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="parentEntity">Reference to the parent entity.</param>
        /// <param name="position">Desired move position.</param>
        public static void MoveCompound(Frame f, EntityRef parentEntity, FPVector2 position, FP rotation)
        {
            s_battleDebugLogger.DevAssertFormat(
                f,
                f.Has<BattleCompoundEntityComponent>(parentEntity),
                "Parent entity ({0}) is expected to have a BattleCompoundEntityComponent",
                parentEntity
            );
            if (!f.Has<BattleCompoundEntityComponent>(parentEntity)) return;

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(parentEntity);
            parentTransform->Position = position;
            parentTransform->Rotation = rotation;

            BattleCompoundEntityComponent compound = f.Get<BattleCompoundEntityComponent>(parentEntity);
            QList<BattleEntityLink> linkedEntities = f.ResolveList(compound.LinkedEntities);

            for(int i = 0; i < linkedEntities.Count; i++)
            {
                EntityRef childEntity = linkedEntities[i].ERef;
                if (!f.Exists(childEntity)) continue;

                Transform2D* childTransform = f.Unsafe.GetPointer<Transform2D>(childEntity);
                childTransform->Position = CalculateWorldPosition(
                        parentTransform->Position,
                        parentTransform->Rotation,
                        linkedEntities[i].Offset
                    );
                childTransform->Rotation = parentTransform->Rotation;
            }
        }

        /// <summary>
        /// Public method for teleporting compound.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="parentEntity">Reference to the parent entity.</param>
        /// <param name="position">Desired teleport position.</param>
        public static void TeleportCompound(Frame f, EntityRef parentEntity, FPVector2 position, FP rotation)
        {
            s_battleDebugLogger.DevAssertFormat(
                f,
                f.Has<BattleCompoundEntityComponent>(parentEntity),
                "Parent entity ({0}) is expected to have a BattleCompoundEntityComponent",
                parentEntity
            );
            if (!f.Has<BattleCompoundEntityComponent>(parentEntity)) return;

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(parentEntity);
            parentTransform->Teleport(f, position, rotation);

            BattleCompoundEntityComponent compound = f.Get<BattleCompoundEntityComponent>(parentEntity);
            QList<BattleEntityLink> linkedEntities = f.ResolveList(compound.LinkedEntities);

            for (int i = 0; i < linkedEntities.Count; i++)
            {
                EntityRef childEntity = linkedEntities[i].ERef;
                if (!f.Exists(childEntity)) continue;

                Transform2D* childTransform = f.Unsafe.GetPointer<Transform2D>(childEntity);
                childTransform->Teleport(
                    f,
                    CalculateWorldPosition(
                        parentTransform->Position,
                        parentTransform->Rotation,
                        linkedEntities[i].Offset
                    ),
                    parentTransform->Rotation
                );
            }
        }

        /// <summary>
        /// Public method for making compounds.
        /// </summary>
        ///
        /// Used when registering compounds using RegisterCompound methods.<br/>
        /// @clink{RegisterCompound:BattleEntityManager.RegisterCompound(Frame, CompoundEntityTemplate)}<br/>
        /// @clink{RegisterCompound:BattleEntityManager.RegisterCompound(Frame, CompoundEntityTemplate[])}
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="template">Compound entity template used to make compounds.</param>
        public static void MakeCompound(Frame f, CompoundEntityTemplate template)
        {
            BattleCompoundEntityComponent compound = new BattleCompoundEntityComponent();

            compound.LinkedEntities = f.AllocateList<BattleEntityLink>(template.LinkedEntities.Count);
            QList<BattleEntityLink> entityList = f.ResolveList(compound.LinkedEntities);

            foreach(BattleEntityLink entityRef in template.LinkedEntities)
            {
                entityList.Add(entityRef);
            }

            f.Set(template.ParentEntityRef, compound);
        }

        private static BattleDebugLogger s_battleDebugLogger;

        /// <summary>
        /// Private helper method for getting the BattleEntityManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer reference to the BattleEntityManagerData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleEntityManagerDataQSingleton* GetEntityManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattleEntityManagerDataQSingleton* entityManagerData))
            {
                s_battleDebugLogger.Error(f, "EntityManagerData singleton not found!");
            }

            return entityManagerData;
        }

        private static FPVector2 CalculateWorldPosition(FPVector2 parentPosition, FP parentRotation, FPVector2 offset)
        {
            return FPVector2.Rotate(offset, parentRotation) + parentPosition;
        }
    }
}
