using System.Runtime.CompilerServices;
using Quantum;
using Quantum.Collections;

namespace Quantum
{
    public partial struct BattleEntityID
    {
        public static implicit operator int(BattleEntityID id) => id.Int;
        public static explicit operator BattleEntityID(int index) => new BattleEntityID() { Int = index };
    }
}

namespace Battle.QSimulation.Game
{
    public static unsafe class BattleEntityManager
    {
        public static void Init(Frame f, BattleGridPosition entityOffscreenPositionOffset, int entitySpacing)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);

            entityManagerData->RegisteredEntities = f.AllocateList<EntityRef>();

            entityManagerData->EntityOffscreenPositionOffset = entityOffscreenPositionOffset;
            entityManagerData->EntitySpacing = entitySpacing;
        }

        public static BattleEntityID Register(Frame f, EntityRef entity)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            BattleEntityID id = new BattleEntityID() { Int = entityList.Count };
            entityList.Add(entity);
            Return(f, entityList, entity, id);

            return id;
        }

        public static EntityRef Get(Frame f, BattleEntityID id)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);

            return entityList[id];
        }

        public static void Return(Frame f, BattleEntityID id)
        {
            QList<EntityRef> entityList = f.ResolveList(GetEntityManagerData(f)->RegisteredEntities);
            EntityRef entity = entityList[id];

            Return(f, entityList, entity, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Return(Frame f, QList<EntityRef> entityList, EntityRef entity, BattleEntityID id)
        {
            BattleEntityManagerDataQSingleton* entityManagerData = GetEntityManagerData(f);

            BattleGridPosition entityGridPosition = entityManagerData->EntityOffscreenPositionOffset;
            entityGridPosition.Row -= id * entityManagerData->EntitySpacing;
            f.Unsafe.GetPointer<Transform2D>(entity)->Teleport(f, BattleGridManager.GridPositionToWorldPosition(entityGridPosition));
        }

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
