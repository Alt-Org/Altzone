using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Diamond
{
    [Preserve]
    public unsafe class BattleDiamondQSystem : SystemMainThreadFilter<BattleDiamondQSystem.Filter>, ISignalBattleOnDiamondHitPlayer
    {
        public struct Filter
        {
            public EntityRef Entity;
            public BattleDiamondDataQComponent* DiamondData;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // reduce diamond's lifetime
            filter.DiamondData->TimeUntilDisappearance -= FP._0_01;

            if (filter.DiamondData->TimeUntilDisappearance < FP._0) f.Destroy(filter.Entity);
        }

        public void BattleOnDiamondHitPlayer(Frame f, BattleDiamondDataQComponent* diamond, EntityRef diamondEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            BattleDiamondCounterQSingleton* diamondCounter = f.Unsafe.GetPointerSingleton<BattleDiamondCounterQSingleton>();
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            // increase right team's diamondcounter
            if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha) diamondCounter->AlphaDiamonds++;
            else diamondCounter->BetaDiamonds++;

            f.Destroy(diamondEntity);
        }
    }
}