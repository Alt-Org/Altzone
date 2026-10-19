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
        public const int NoneValue = 0;

        public static BattleEntityID None { get => new() { Int = NoneValue }; }
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

            BattleEntityID id = AssignID(entityList);
            int listIndex = GetListIndex(id);

            MakeManagedEntity(f, entityRef);
            entityList.Add(entityRef);
            Return(f, entityManagerData, entityRef, listIndex);

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

            BattleEntityID id = AssignID(entityList);
            int listIndex = GetListIndex(id);

            for (int i = 0; i < entityRefs.Length; i++)
            {
                MakeManagedEntity(f, entityRefs[i]);
                entityList.Add(entityRefs[i]);
                Return(f, entityManagerData, entityRefs[i], listIndex);
                listIndex++;
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

            BattleEntityID id = AssignID(entityList);
            int listIndex = GetListIndex(id);

            MakeManagedEntity(f, template, registered: true);
            entityList.Add(template.ParentEntityRef);
            Return(f, entityManagerData, template.ParentEntityRef, listIndex);

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

            BattleEntityID id = AssignID(entityList);
            int listIndex = GetListIndex(id);

            for (int i = 0; i < templates.Length; i++)
            {
                CompoundEntityTemplate template = templates[i];

                MakeManagedEntity(f, template, registered: true);
                entityList.Add(template.ParentEntityRef);
                Return(f, entityManagerData, template.ParentEntityRef, listIndex);
                listIndex++;
            }

            return id;
        }

        #endregion Public Register Methods
        /// @}

        /// @anchor BattleEntityManager-GetReturnMethods
        /// @name Get/Return Methods
        /// Methods for retrieving and returning [{Registered Entities}](#page-concepts-entity-management-registered-entities).
        /// @{
        #region Public Get Methods

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
        public static BattleManageEntityHandle Get(Frame f, BattleEntityID id)
        {
            DevAssertValidEntityID(id, context: nameof(Get));

            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);
            int listIndex = GetListIndex(id);

            return BattleManageEntityHandle.Create(f, entityList[listIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(Frame f, BattleEntityID id) where T : BattleManageEntityHandle.IComposite, new()
        {
            return Get(f, id).To<T>(f);
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
        ///
        /// <returns>EntityRef for retrieved entity.</returns>
        public static BattleManageEntityHandle Get(Frame f, BattleEntityID id, int offset)
        {
            DevAssertValidEntityID(id, context: nameof(Get));

            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);
            int listIndex = GetListIndex(id, offset);

            return BattleManageEntityHandle.Create(f, entityList[listIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(Frame f, BattleEntityID id, int offset) where T : BattleManageEntityHandle.IComposite, new()
        {
            return Get(f, id, offset).To<T>(f);
        }

        #endregion Public Get Methods
        /// @}

        /// @anchor BattleEntityManager-GetReturnMethods
        /// @name Get/Return Methods
        /// Methods for retrieving and returning [{Registered Entities}](#page-concepts-entity-management-registered-entities).
        /// @{
        #region Public Use/Return Methods

        public static void Use(Frame f, BattleEntityID id)
        {
            DevAssertValidEntityID(id, context: nameof(Use));

            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);
            int listIndex = GetListIndex(id);

            BattleManageEntityQComponent* managedEntity = f.Unsafe.GetPointer<BattleManageEntityQComponent>(entityList[listIndex]);

            managedEntity->RegisteredState = BattleEntityPlayState.InPlay;
            f.Events.BattlePlayStateUpdate(entityList[listIndex], managedEntity->RegisteredState);
        }

        public static void Use(Frame f, BattleEntityID id, int offset)
        {
            DevAssertValidEntityID(id, context: nameof(Use));

            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);
            int listIndex = GetListIndex(id, offset);

            BattleManageEntityQComponent* managedEntity = f.Unsafe.GetPointer<BattleManageEntityQComponent>(entityList[listIndex]);

            managedEntity->RegisteredState = BattleEntityPlayState.InPlay;
            f.Events.BattlePlayStateUpdate(entityList[listIndex], managedEntity->RegisteredState);
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
            DevAssertValidEntityID(id, context: nameof(Return));

            int listIndex = GetListIndex(id);

            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            EntityRef entityRef = f.ResolveList(entityManagerData->RegisteredEntities)[listIndex];

            Return(f, entityManagerData, entityRef, listIndex);
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
            DevAssertValidEntityID(id, context: nameof(Return));

            int listIndex = GetListIndex(id, offset);

            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            EntityRef entityRef = f.ResolveList(entityManagerData->RegisteredEntities)[listIndex];

            Return(f, entityManagerData, entityRef, listIndex);
        }

        #endregion Public Use/Return Methods
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
        public static void MakeCompound(Frame f, CompoundEntityTemplate template) => MakeManagedEntity(f, template, registered: false);

        /// <summary>
        /// Moves a Compound Entity.
        /// </summary>
        /// Part of @ref BattleEntityManager-CompoundMethods "Compound methods"
        ///
        /// See [{Compound Entities}](#page-concepts-entity-management-compound-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="managedEntity">Reference to the parent entity.</param>
        /// <param name="position">New position.</param>
        /// <param name="rotation">New rotation</param>
        public static void MoveCompound(Frame f, BattleManageEntityHandle managedEntity, FPVector2 position, FP rotation)
        {
            managedEntity.DevAssertIsCompound(nameof(BattleEntityManager), "Can not " + nameof(MoveCompound));

            BattleDebugLogger.DevDebugCode(() => { if (managedEntity.IsRegistered)
            {
                managedEntity.DevAssertIsInPlay(nameof(BattleEntityManager), "Can not" + nameof(MoveCompound));
            }});

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(managedEntity);
            parentTransform->Position = position;
            parentTransform->Rotation = rotation;

            QList<BattleEntityLink> linkedEntities = managedEntity.CompoundGetLinkedEntities(f);

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
        /// <param name="managedEntity">Reference to the parent entity.</param>
        /// <param name="position">New position.</param>
        /// <param name="rotation">New rotation</param>
        public static void TeleportCompound(Frame f, BattleManageEntityHandle managedEntity, FPVector2 position, FP rotation)
        {
            managedEntity.DevAssertIsCompound(nameof(BattleEntityManager), "Can not " + nameof(TeleportCompound));

            BattleDebugLogger.DevDebugCode(() => { if (managedEntity.IsRegistered)
            {
                managedEntity.DevAssertIsInPlay(nameof(BattleEntityManager), "Can not" + nameof(TeleportCompound));
            }});

            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(managedEntity);
            parentTransform->Teleport(f, position, rotation);

            QList<BattleEntityLink> linkedEntities = managedEntity.CompoundGetLinkedEntities(f);

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

        #region Private Id Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleEntityID AssignID(QList<EntityRef> entityList)
        {
            return new() { Int = entityList.Count + 1};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DevAssertValidEntityID(BattleEntityID id, string context)
        {
            s_debugLogger.DevAssertFormat(id.Int != BattleEntityID.NoneValue, "Invalid ID, Can not {0} none EntityID", context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetListIndex(BattleEntityID id)
        {
            return id.Int - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetListIndex(BattleEntityID id, int offset)
        {
            return id.Int + offset - 1;
        }

        #endregion Private Id Methods

        #region Private Entity Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleManageEntityQComponent* MakeManagedEntity(Frame f, EntityRef entityRef)
        {
            BattleManageEntityQComponent managedEntity = new();

            managedEntity.IsRegistered = true;
            managedEntity.RegisteredState = BattleEntityPlayState.InPlay;

            f.Add(entityRef, managedEntity, out BattleManageEntityQComponent* componentPointer);

            return componentPointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleManageEntityQComponent* MakeManagedEntity(Frame f, CompoundEntityTemplate template,  bool registered)
        {
            BattleManageEntityQComponent managedEntity = new();

            if (registered)
            {
                managedEntity.IsRegistered = true;
                managedEntity.RegisteredState = BattleEntityPlayState.InPlay;
            }

            managedEntity.IsCompound = true;
            managedEntity.CompoundLinkedEntities = f.AllocateList<BattleEntityLink>(template.LinkedEntities.Count);
            QList<BattleEntityLink> entityList = f.ResolveList(managedEntity.CompoundLinkedEntities);

            foreach (BattleEntityLink entityRef in template.LinkedEntities)
            {
                entityList.Add(entityRef);
            }

            f.Add(template.ParentEntityRef, managedEntity, out BattleManageEntityQComponent* componentPointer);

            return componentPointer;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Return(Frame f, BattleEntityManagerDataQSingleton* entityManagerData, EntityRef entityRef, int listIndex)
        {
            BattleGridPosition entityGridPosition = entityManagerData->EntityOffscreenPositionOffset;
            entityGridPosition.Row -= listIndex * entityManagerData->EntitySpacing;

            FPVector2 entityGridToWorldPosition = BattleGridManager.GridPositionToWorldPosition(entityGridPosition);

            BattleManageEntityHandle managedEntity = BattleManageEntityHandle.Create(f, entityRef);

            if (managedEntity.IsCompound)
            {
                TeleportCompound(f, managedEntity, entityGridToWorldPosition, 0);
            }
            else
            {
                f.Unsafe.GetPointer<Transform2D>(entityRef)->Teleport(f, entityGridToWorldPosition);
            }

            f.Unsafe.GetPointer<BattleManageEntityQComponent>(managedEntity)->RegisteredState = BattleEntityPlayState.OutOfPlay;
            f.Events.BattlePlayStateUpdate(entityRef, managedEntity.RegisteredState);
        }

        #endregion Private Entity Methods

        #region Private Util Methods

        /// <summary>
        /// Transforms relative <paramref name="offset"/> to world position based on <paramref name="parentPosition"/> and <paramref name="parentRotation"/>.
        /// </summary>
        ///
        /// <param name="parentPosition">Position of the parent.</param>
        /// <param name="parentRotation">Rotation of the parent.</param>
        /// <param name="offset">Relative offset.</param>
        ///
        /// <returns>The calculated world position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FPVector2 CalculateWorldPosition(FPVector2 parentPosition, FP parentRotation, FPVector2 offset)
        {
            return FPVector2.Rotate(offset, parentRotation) + parentPosition;
        }

        #endregion Private Util Methods
    }
}
