/// @file BattleEntityManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleEntityManager} class which handles %Quantum entity management in the game.<br/>
/// Entities are registered, and can be retrieved and returned.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Quantum.Collections;

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

        /// <summary>
        /// Explicit cast from <c>int</c> implementation.
        /// </summary>
        /// 
        /// <param name="value">The <c>int</c> value to cast to BattleEntityID.</param>
        public static explicit operator BattleEntityID(int value) => new BattleEntityID() { Int = value };
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
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);

            entityManagerData->RegisteredEntities = f.AllocateList<EntityRef>();

            entityManagerData->EntityOffscreenPositionOffset = entityOffscreenPositionOffset;
            entityManagerData->EntitySpacing = entitySpacing;
        }

        /// <summary>
        /// Registers given <paramref name="entity"/> to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        ///
        /// Use @cref{Register(Frame\, EntityRef[])} to register a group of entities.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entity">Entity to register.</param>
        /// 
        /// <returns>Entity ID of the registered entity.</returns>
        public static BattleEntityID Register(Frame f, EntityRef entity)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count };
            entityList.Add(entity);
            Return(f, entityManagerData, entity, id);

            return id;
        }

        /// <summary>
        /// Registers given group of <paramref name="entities"/> to <see cref="Quantum.BattleEntityManagerDataQSingleton.RegisteredEntities">RegisteredEntities</see>
        /// </summary>
        ///
        /// Use @cref{Register(Frame\, EntityRef)} to register a single entity.
        ///
        /// See [{Entity ID}](#page-concepts-entity-management-entity-id) for more info.<br/>
        /// See [{Entity Group}](#page-concepts-entity-management-entity-group) for more info.
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entities">Array of entities to register.</param>
        /// 
        /// <returns>Entity ID of the registered entity group.</returns>
        public static BattleEntityID Register(Frame f, EntityRef[] entities)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            QList<EntityRef> entityList = f.ResolveList(entityManagerData->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count };

            for (int i = 0; i < entities.Length; i++)
            {
                entityList.Add(entities[i]);
                Return(f, entityManagerData, entities[i], (BattleEntityID)(id + i));
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
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);
            EntityRef entity = f.ResolveList(entityManagerData->RegisteredEntities)[id];

            Return(f, entityManagerData, entity, (BattleEntityID)(id + offset));
        }

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
            f.Unsafe.GetPointer<Transform2D>(entity)->Teleport(f, BattleGridManager.GridPositionToWorldPosition(entityGridPosition));
        }

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
                BattleDebugLogger.Error(f, nameof(BattleEntityManager), "EntityManagerData singleton not found!");
            }

            return entityManagerData;
        }
    }
}
