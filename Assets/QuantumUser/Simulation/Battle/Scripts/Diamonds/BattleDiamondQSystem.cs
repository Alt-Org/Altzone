/// <summary>
/// @file BattleDiamondQSystem.cs
/// @brief Handles spawning, managing and destroying diamonds.
///
/// This system spawns diamonds when it receices a signal, filters all diamond entities and handles their lifetime, and destroys diamonds
/// when player collects them by colliding with them or if diamond's lifetime ends.
/// </summary>
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Diamond
{
    /// <summary>
    /// <span class="brief-h">Diamond <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles spawning diamonds, managing their lifetime and destroying them.
    /// </summary>
    [Preserve]
    public unsafe class BattleDiamondQSystem : SystemMainThreadFilter<BattleDiamondQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnDiamondHitPlayer
    {
        /// <summary>
        /// Filter for filtering diamond entities.
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public BattleDiamondDataQComponent* DiamondData;
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitSoulWall">ISignalBattleOnProjectileHitSoulWall</see> is sent.</span><br/>
        /// Calls private <see cref="CreateDiamonds(Frame, FPVector2, BattleDiamondQSpec)">CreateDiamonds</see> method when projectile hits a SoulWall.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current Quantum Frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="soulWall">Pointer to the SoulWall component.</param>
        /// <param name="soulWallEntity">EntityRef of the SoulWall.</param>
        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            BattleDiamondQSpec diamondSpec = BattleQConfig.GetDiamondSpec(f);

            CreateDiamonds(f, soulWall->Normal, diamondSpec);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Manages each diamond's lifetime and destroys them if players don't gather them quickly enough.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        /// <param name="f">Current Quantum Frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            // reduce diamond's lifetime
            filter.DiamondData->TimeUntilDisappearance -= FP._0_01;

            if (filter.DiamondData->TimeUntilDisappearance < FP._0) f.Destroy(filter.Entity);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnDiamondHitPlayer">ISignalBattleOnDiamondHitPlayer</see> is sent.</span><br/>
        /// Destroys diamonds when player hits them and increases diamondcounters.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current Quantum Frame.</param>
        /// <param name="diamond">Pointer to the diamond component.</param>
        /// <param name="diamondEntity">EntityRef of the diamond.</param>
        /// <param name="playerHitbox">Pointer to the playerHitbox component.</param>
        /// <param name="playerEntity">EntityRef of the player.</param>
        public void BattleOnDiamondHitPlayer(Frame f, BattleDiamondDataQComponent* diamond, EntityRef diamondEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            BattleDiamondCounterQSingleton* diamondCounter = f.Unsafe.GetPointerSingleton<BattleDiamondCounterQSingleton>();
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            // increase right team's diamondcounter
            if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha) diamondCounter->AlphaDiamonds++;
            else diamondCounter->BetaDiamonds++;

            f.Destroy(diamondEntity);
        }

        /// <summary>
        /// Creates diamonds and teleports them to a random position on scoring team's side of the arena.
        /// </summary>
        /// <param name="f">Current Quantum Frame.</param>
        /// <param name="wallNormal">Normal of the SoulWall.</param>
        /// <param name="diamondSpec">The DiamondSpec.</param>
        private void CreateDiamonds(Frame f, FPVector2 wallNormal, BattleDiamondQSpec diamondSpec)
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