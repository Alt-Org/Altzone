using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Diamond
{
    [Preserve]
    public unsafe class BattleDiamondSpawnerQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitSoulWall
    {
        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            BattleDiamondQSpec diamondSpec = BattleQConfig.GetDiamondSpec(f);

            CreateDiamonds(f, soulWall->Normal, diamondSpec);
        }

        public void CreateDiamonds(Frame f, FPVector2 wallNormal, BattleDiamondQSpec diamondSpec)
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
