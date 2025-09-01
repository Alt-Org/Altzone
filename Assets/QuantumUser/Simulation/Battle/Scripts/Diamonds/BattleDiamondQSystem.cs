using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Game;

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

        public static void OnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall)
        {
            BattleDiamondQSpec diamondSpec = BattleQConfig.GetDiamondSpec(f);

            CreateDiamonds(f, soulWall->Normal, diamondSpec);
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

        private static void CreateDiamonds(Frame f, FPVector2 wallNormal, BattleDiamondQSpec diamondSpec)
        {
            // diamond temp variables
            BattleGridPosition diamondRandomPosition;
            FPVector2          diamondPosition;
            BattleTeamNumber   diamondOwnerTeam;
            int                spawnAreaStart;
            int                spawnAreaEnd;

            // set diamond common temp variables (used for all diamonds)
            diamondOwnerTeam = wallNormal.Y == FP._1 ? BattleTeamNumber.TeamBeta : BattleTeamNumber.TeamAlpha;
            spawnAreaStart   = diamondOwnerTeam == BattleTeamNumber.TeamAlpha ? BattleGridManager.TeamAlphaFieldStart : BattleGridManager.TeamBetaFieldStart;
            spawnAreaEnd     = diamondOwnerTeam == BattleTeamNumber.TeamAlpha ? BattleGridManager.TeamAlphaFieldEnd   : BattleGridManager.TeamBetaFieldEnd;

            // diamond variables
            EntityRef                    diamondEntity;
            BattleDiamondDataQComponent* diamondData;
            Transform2D*                 diamondTransform;

            for(int i = 0; i < diamondSpec.SpawnAmount; i++)
            {
                // randomize diamond's spawn position inside allowed spawn area
                diamondRandomPosition = new BattleGridPosition()
                {
                    Col = f.RNG->NextInclusive(0, BattleGridManager.Columns-1),
                    Row = f.RNG->NextInclusive(spawnAreaStart, spawnAreaEnd)
                };

                diamondPosition = BattleGridManager.GridPositionToWorldPosition(diamondRandomPosition);

                // create diamond
                diamondEntity = f.Create(diamondSpec.DiamondPrototype);

                // get diamondData component
                diamondData = f.Unsafe.GetPointer<BattleDiamondDataQComponent>(diamondEntity);

                // initialize diamondData component
                diamondData->OwnerTeam = diamondOwnerTeam;
                diamondData->TimeUntilDisappearance = FP._4;

                // teleport diamond to spawn position
                diamondTransform = f.Unsafe.GetPointer<Transform2D>(diamondEntity);
                diamondTransform->Teleport(f, diamondPosition, FP._0);
            }
        }
    }
}
