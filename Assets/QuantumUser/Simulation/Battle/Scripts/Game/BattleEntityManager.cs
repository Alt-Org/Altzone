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
    // Main struct definition in BattleEntityID.qtn
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
    /// See [{Entity Manager}](#page-concepts-entity-management-entity-manager) for more info.
    ///
    /// [{Registered Entities}](#page-concepts-entity-management-registered-entities) are stored offscreen when [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate)
    /// and are assigned an [{Entity ID}](#page-concepts-entity-management-entity-id) for accessing them.<br/>
    /// Entities can be Retrieved using
    /// @clink{Get:Battle.QSimulation.Game.BattleEntityManager.Get(Frame, BattleEntityID, bool)} to be used in the arena. [{InPlay}](#page-concepts-entity-management-registered-entities-playstate)<br/>
    /// Entities can be Returned using
    /// @clink{Return:Battle.QSimulation.Game.BattleEntityManager.Return(Frame, BattleEntityID)}, teleporting them back offscreen. [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate)
    ///
    /// The @cref{Quantum,BattleEntityManagerDataQSingleton} struct is used internally to manage [{Registered Entities}](#page-concepts-entity-management-registered-entities).
    public static unsafe class BattleEntityManager
    {
        /// <summary>
        /// Public struct used as template when making compounds with <see cref="BattleEntityManager"/>.
        /// </summary>
        ///
        /// See @cref{BattleEntityManager.MakeCompound}<br/>
        /// See @cref{BattleEntityManager.RegisterCompound(Frame\, CompoundEntityTemplate)}<br/>
        /// See @cref{BattleEntityManager.RegisterCompound(Frame\, CompoundEntityTemplate[])}
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.
        public struct CompoundEntityTemplate
        {
            /// <summary>Parent entity reference of the compound.</summary>
            public EntityRef ParentEntityRef;

            /// <summary>List containing entities that are linked to compound parent.</summary>
            public List<BattleEntityLink> LinkedEntities;

            /// <summary>
            /// Creates new compound template.<br/>
            /// Use <see cref="Link"/> to link other entities.
            /// </summary>
            ///
            /// <param name="parent">Parent entity reference of the compound.</param>
            ///
            /// <returns>New CompoundEntityTemplate.</returns>
            public static CompoundEntityTemplate Create(EntityRef parent)
            {
                return new CompoundEntityTemplate
                {
                    ParentEntityRef = parent,
                    LinkedEntities = new List<BattleEntityLink>()
                };
            }

            /// <summary>
            /// Creates new compound template with specified <paramref name="capacity"/>.<br/>
            /// Use <see cref="Link"/> to link other entities.
            /// </summary>
            ///
            /// <param name="parent">Parent entity reference of the compound.</param>
            /// <param name="capacity">Capacity used to create linked entities list.</param>
            ///
            /// <returns>New CompoundEntityTemplate.</returns>
            public static CompoundEntityTemplate Create(EntityRef parent, int capacity)
            {
                return new CompoundEntityTemplate
                {
                    ParentEntityRef = parent,
                    LinkedEntities = new List<BattleEntityLink>(capacity)
                };
            }

            /// <summary>
            /// Links entity to compound parent.
            /// </summary>
            ///
            /// <param name="entityRef">The entity reference to be linked.</param>
            /// <param name="offset">Linked entity's position relative to compound parent.</param>
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
            s_debugLogger = BattleDebugLogger.Create(typeof(BattleEntityManager));

            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);

            entityManagerData->RegisteredEntities = f.AllocateList<EntityRef>();

            entityManagerData->EntityOffscreenPositionOffset = entityOffscreenPositionOffset;
            entityManagerData->EntitySpacing = entitySpacing;
        }

        /// @anchor BattleEntityManager-RegisterMethods
        /// @name Register Methods
        /// Methods for [{Registering}](#page-concepts-entity-management-registered-entities) entityrefs.
        /// @{
        #region Public Register Methods

        /// <summary>
        /// Registers given <paramref name="entityRef"/>
        /// to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        /// Part of @ref BattleEntityManager-RegisterMethods "Register Methods"
        ///
        /// Use @cref{Register(Frame\, EntityRef[])} to register a group of entities.
        ///
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
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

            BattleEntityID id = new() { Int = entityList.Count };
            entityList.Add(entityRef);
            Return(f, entityManagerData, entityRef, id);

            return id;
        }

        /// <summary>
        /// Registers given group of <paramref name="entityRefs"/>
        /// to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        /// Part of @ref BattleEntityManager-RegisterMethods "Register Methods"
        ///
        /// Use @cref{Register(Frame\, EntityRef)} to register a single entity.
        ///
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
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

            BattleEntityID id = new() { Int = entityList.Count };
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
        /// Makes and registers given compound entity based on <paramref name="template"/>
        /// to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        /// Part of @ref BattleEntityManager-RegisterMethods "Register Methods"
        ///
        /// Use @cref{RegisterCompound(Frame\, CompoundEntityTemplate[])} to register and make a group of compound entities.<br/>
        /// Use @cref{MakeCompound} method to make non-registered compound.<br/>
        /// Use @cref{Register(Frame\, EntityRef)} to register a non-compound entity.
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        ///
        /// Uses @cref{MakeCompound} method to make compounds.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="template">Compound entity template used to make compounds</param>
        ///
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID RegisterCompound(Frame f, CompoundEntityTemplate template)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new() { Int = entityList.Count, IsCompound = true };
            BattleEntityID offsetId = id;

            MakeCompound(f, template);

            EntityRef entityRef = template.ParentEntityRef;
            entityList.Add(entityRef);
            Return(f, entityManagerData, entityRef, offsetId);
            offsetId.Int++;

            return id;
        }

        /// <summary>
        /// Makes and registers given group of compound entities based on <paramref name="templates"/>
        /// to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        /// Part of @ref BattleEntityManager-RegisterMethods "Register Methods"
        ///
        /// Use @cref{RegisterCompound(Frame\, CompoundEntityTemplate)} to register and make a single compound entity.<br/>
        /// Use @cref{MakeCompound} method to make non-registered compound.<br/>
        /// Use @cref{Register(Frame\, EntityRef[])} to register a group of non-compound entities.
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.
        ///
        /// Uses @cref{MakeCompound} method to make compounds.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="templates">Compound entity templates used to make compounds</param>
        ///
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID RegisterCompound(Frame f, CompoundEntityTemplate[] templates)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new() { Int = entityList.Count, IsCompound = true };
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

        #endregion Public Register Methods
        /// @}

        /// @anchor BattleEntityManager-GetReturnMethods
        /// @name Get/Return Methods
        /// Methods for retrieving and returning [{Registered Entities}](#page-concepts-entity-management-registered-entities).
        /// @{
        #region Public Get/Return Methods

        /// <summary>
        /// Retrieves registered entity matching given <paramref name="id"/>.
        /// </summary>
        /// Part of @ref BattleEntityManager-GetReturnMethods "Get/Return methods"
        ///
        /// Use @cref{Get(Frame\, BattleEntityID\, int\, bool)} to retreive an entity in a group.
        ///
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{PlayState}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to retrieve.</param>
        /// <param name="updateViewPlayState">Whether to update the ViewPlayState.</param>
        ///
        /// <returns>EntityRef for retrieved entity.</returns>
        public static EntityRef Get(Frame f, BattleEntityID id, bool updateViewPlayState = false)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            if (updateViewPlayState) f.Events.BattlePlayStateUpdate(entityList[id], IsInPlay: true);

            return entityList[id];
        }

        /// <summary>
        /// Retrieves  registered entity matching given <paramref name="id"/> from the group at <paramref name="offset"/>.
        /// </summary>
        /// Part of @ref BattleEntityManager-GetReturnMethods "Get/Return methods"
        ///
        /// Use @cref{Get(Frame\, BattleEntityID\, bool)} to retreive an entity not in any group.
        ///
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.<br/>
        /// See [{PlayState}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="id">Entity ID of the entity to retrieve.</param>
        /// <param name="offset">Offset of the desired entity within the group.</param>
        /// <param name="updateViewPlayState">Whether to update the ViewPlayState.</param>
        ///
        /// <returns>EntityRef for retrieved entity.</returns>
        public static EntityRef Get(Frame f, BattleEntityID id, int offset, bool updateViewPlayState = false)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            id.Int += offset;

            if (updateViewPlayState) f.Events.BattlePlayStateUpdate(entityList[id], IsInPlay: true);

            return entityList[id];
        }

        /// <summary>
        /// Returns registered entity matching given <paramref name="id"/> back offscreen.
        /// </summary>
        /// Part of @ref BattleEntityManager-GetReturnMethods "Get/Return methods"
        ///
        /// Use @cref{Return(Frame\, BattleEntityID\, int)} to return an entity in a group.
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
            EntityRef entityRef = f.ResolveList(entityManagerData->RegisteredEntities)[id];

            Return(f, entityManagerData, entityRef, id);
        }

        /// <summary>
        /// Returns registered entity matching given <paramref name="id"/> from the group at <paramref name="offset"/> back offscreen.
        /// </summary>
        /// Part of @ref BattleEntityManager-GetReturnMethods "Get/Return methods"
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

            EntityRef entityRef = EntityRef.None;
            try
            {
                entityRef = f.ResolveList(entityManagerData->RegisteredEntities)[id];
            }
            catch (System.Exception)
            {
                int test = 0;
            }
            //EntityRef entityRef = f.ResolveList(entityManagerData->RegisteredEntities)[id];

            Return(f, entityManagerData, entityRef, id);
        }

        #endregion Public Get/Return Methods
        /// @}

        /// @anchor BattleEntityManager-CompoundMethods
        /// @name Compound Methods
        /// Methods for [{Handling Compound Entities}](#page-concepts-entity-management-compound-entity-handling).
        /// @{
        #region Public Compound Methods

        /// <summary>
        /// Makes a compound entity based on <paramref name="template"/>.
        /// </summary>
        /// Part of @ref BattleEntityManager-CompoundMethods "Compound methods"
        ///
        /// Use @cref{RegisterCompound(Frame\, CompoundEntityTemplate)} to make and register a single compound entity.<br/>
        /// Use @cref{RegisterCompound(Frame\, CompoundEntityTemplate[])} to make and register a group of compound entities.<br/>
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.
        ///
        /// Used internally when registering compounds using RegisterCompound methods.<br/>
        /// @cref{RegisterCompound(Frame\, CompoundEntityTemplate)}<br/>
        /// @cref{RegisterCompound(Frame\, CompoundEntityTemplate[])}
        ///
        /// @note
        /// Registered Compound Entities should only be made using
        /// @clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} methods
        /// which marks the resulting [{Entity ID}](#page-concepts-entity-management-entity-id) as a Compound allowing the Entity Manager to handle it correctly.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="template">Compound entity template used to make compounds.</param>
        public static void MakeCompound(Frame f, CompoundEntityTemplate template)
        {
            BattleCompoundEntityQComponent compound = new();
            compound.LinkedEntities = f.AllocateList<BattleEntityLink>(template.LinkedEntities.Count);
            QList<BattleEntityLink> entityList = f.ResolveList(compound.LinkedEntities);

            foreach (BattleEntityLink entityRef in template.LinkedEntities)
            {
                entityList.Add(entityRef);
            }

            f.Set(template.ParentEntityRef, compound);
        }

        /// <summary>
        /// Moves a Compound Entity.
        /// </summary>
        /// Part of @ref BattleEntityManager-CompoundMethods "Compound methods"
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="parentEntityRef">Reference to the parent entity.</param>
        /// <param name="position">New position.</param>
        /// <param name="rotation">New rotation</param>
        public static void MoveCompound(Frame f, EntityRef parentEntityRef, FPVector2 position, FP rotation)
        {
            s_debugLogger.DevAssertFormat(
                f,
                f.Has<BattleCompoundEntityQComponent>(parentEntityRef),
                "Parent entity ({0}) is expected to have a BattleCompoundEntityComponent",
                parentEntityRef
            );
            if (!f.Has<BattleCompoundEntityQComponent>(parentEntityRef)) return;

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(parentEntityRef);
            parentTransform->Position = position;
            parentTransform->Rotation = rotation;

            BattleCompoundEntityQComponent compound = f.Get<BattleCompoundEntityQComponent>(parentEntityRef);
            QList<BattleEntityLink> linkedEntities = f.ResolveList(compound.LinkedEntities);

            for(int i = 0; i < linkedEntities.Count; i++)
            {
                EntityRef linkedEntity = linkedEntities[i].ERef;
                if (!f.Exists(linkedEntity)) continue;

                Transform2D* linkedTransform = f.Unsafe.GetPointer<Transform2D>(linkedEntity);
                linkedTransform->Position = CalculateWorldPosition(
                        parentTransform->Position,
                        parentTransform->Rotation,
                        linkedEntities[i].Offset
                    );
                linkedTransform->Rotation = parentTransform->Rotation;
            }
        }

        /// <summary>
        /// Teleports a Compound Entity.
        /// </summary>
        /// Part of @ref BattleEntityManager-CompoundMethods "Compound methods"
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="parentEntityRef">Reference to the parent entity.</param>
        /// <param name="position">New position.</param>
        /// <param name="rotation">New rotation</param>
        public static void TeleportCompound(Frame f, EntityRef parentEntityRef, FPVector2 position, FP rotation)
        {
            s_debugLogger.DevAssertFormat(
                f,
                f.Has<BattleCompoundEntityQComponent>(parentEntityRef),
                "Parent entity ({0}) is expected to have a BattleCompoundEntityComponent",
                parentEntityRef
            );
            if (!f.Has<BattleCompoundEntityQComponent>(parentEntityRef)) return;

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(parentEntityRef);
            parentTransform->Teleport(f, position, rotation);

            BattleCompoundEntityQComponent compound = f.Get<BattleCompoundEntityQComponent>(parentEntityRef);
            QList<BattleEntityLink> linkedEntities = f.ResolveList(compound.LinkedEntities);

            for (int i = 0; i < linkedEntities.Count; i++)
            {
                EntityRef linkedEntity = linkedEntities[i].ERef;
                if (!f.Exists(linkedEntity)) continue;

                Transform2D* linkedTransform = f.Unsafe.GetPointer<Transform2D>(linkedEntity);
                linkedTransform->Teleport(
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
        #endregion Public Compound Methods
        /// @}

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        #region Private Methods

        /// <summary>
        /// Private helper method for getting the BattleEntityManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// See [{Entity Manager Data}](#page-concepts-entity-management-entity-manager-data) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer to the BattleEntityManagerData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleEntityManagerDataQSingleton* GetEntityManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattleEntityManagerDataQSingleton* entityManagerData))
            {
                s_debugLogger.Error(f, "EntityManagerData singleton not found!");
            }

            return entityManagerData;
        }

        /// <summary>
        /// Private method that handles entity return logic. Used by the public
        /// <see cref="BattleEntityManager.Return(Frame, BattleEntityID)">Return(Frame, BattleEntityID)</see>
        /// and <see cref="BattleEntityManager.Return(Frame, BattleEntityID, int)">Return(Frame, BattleEntityID, int)</see> methods.
        /// </summary>
        ///
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        ///
        /// Also used by the **Register** methods.<br/>
        /// @cref{Register(Frame\, EntityRef)}<br/>
        /// @cref{Register(Frame\, EntityRef[])}<br/>
        /// @cref{RegisterCompound(Frame\, CompoundEntityTemplate)}<br/>
        /// @cref{RegisterCompound(Frame\, CompoundEntityTemplate[])}
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityManagerData">Pointer to BattleEntityManagerData singleton.</param>
        /// <param name="entityRef">The entity to return.</param>
        /// <param name="id">Entity ID of the entity to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Return(Frame f, BattleEntityManagerDataQSingleton* entityManagerData, EntityRef entityRef, BattleEntityID id)
        {
            BattleGridPosition entityGridPosition = entityManagerData->EntityOffscreenPositionOffset;
            entityGridPosition.Row -= id * entityManagerData->EntitySpacing;

            FPVector2 entityGridToWorldPosition = BattleGridManager.GridPositionToWorldPosition(entityGridPosition);

            if (id.IsCompound)
            {
                TeleportCompound(f, entityRef, entityGridToWorldPosition, 0);
            }
            else
            {
                f.Unsafe.GetPointer<Transform2D>(entityRef)->Teleport(f, entityGridToWorldPosition);
            }
            f.Events.BattlePlayStateUpdate(entityRef, IsInPlay: false);
        }

        /// <summary>
        /// Transforms relative <paramref name="offset"/> to world position based on <paramref name="parentPosition"/> and <paramref name="parentRotation"/>.
        /// </summary>
        ///
        /// <param name="parentPosition">Position of the parent.</param>
        /// <param name="parentRotation">Rotation of the parent.</param>
        /// <param name="offset">Relative offset.</param>
        ///
        /// <returns>The calculated world position.</returns>
        private static FPVector2 CalculateWorldPosition(FPVector2 parentPosition, FP parentRotation, FPVector2 offset)
        {
            return FPVector2.Rotate(offset, parentRotation) + parentPosition;
        }
        #endregion Private Methods
    }
}
