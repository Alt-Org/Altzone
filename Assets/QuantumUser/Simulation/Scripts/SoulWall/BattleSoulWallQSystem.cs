using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

using Battle.QSimulation.Projectile;
using Battle.QSimulation.Game;
using Photon.Deterministic;

namespace Battle.QSimulation.SoulWall
{
    [Preserve]
    public unsafe class BattleSoulWallQSystem : SystemSignalsOnly, ISignalOnTriggerProjectileHitSoulWall
    {
        /// <summary>
        /// Creates soulwalls based on BattleArena and SoulWall Specs during map creation phase.<br/>
        /// (this method should only be called once by GameControlSystem during the map creation)
        /// </summary>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="battleArenaSpec">The BattleArenaSpec</param>
        /// <param name="soulWallSpec">The SoulWallSpec</param>
        public static void CreateSoulWalls(Frame f, BattleArenaQSpec battleArenaSpec, BattleSoulWallQSpec soulWallSpec)
        {
            // create soulwall entities on both sides of the arena
            CreateSoulWalls(f, BattleTeamNumber.TeamAlpha, battleArenaSpec.SoulWallTeamAlphaTemplates, soulWallSpec.SoulWallPrototypes);
            CreateSoulWalls(f, BattleTeamNumber.TeamBeta,  battleArenaSpec.SoulWallTeamBetaTemplates,  soulWallSpec.SoulWallPrototypes);
        }

        public void OnTriggerProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");

            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.SoulWall)) return;

            // Destroy the SoulWall entity
            f.Events.PlaySoundEvent(SoundEffect.WallBroken);
            f.Destroy(soulWallEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.SoulWall);
        }

        /// <summary>
        /// Private helper method (for the public <see cref="CreateSoulWalls(Frame, BattleArenaQSpec BattleSoulWallQSpec)">CreateSoulWalls</see>) that creates soulwalls on one side of the arena.
        /// </summary>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="teamNumber">The teamNumber of the team whose side is being created</param>
        /// <param name="soulWallTemplates">An array of soulwall templates that are used to create the soulwalls </param>
        /// <param name="soulWallPrototypes">An array of entityPrototypes that can be created</param>
        private static void CreateSoulWalls(Frame f, BattleTeamNumber teamNumber, BattleSoulWallTemplate[] soulWallTemplates, AssetRef<EntityPrototype>[] soulWallPrototypes)
        {
            // soulwall temp variables
            FPVector2 soulWallPosition;
            int       soulWallEmotionIndex;
            FP        soulWallScale;
            FPVector2 soulWallNormal;
            FPVector2 soulWallColliderExtents;

            // set all soulwall common temp variables (used for all soulwalls on this side)
            soulWallScale  = BattleGridManager.GridScaleFactor;
            soulWallNormal = new FPVector2(0, teamNumber == BattleTeamNumber.TeamAlpha ? FP._1 : FP.Minus_1);

            // soulwall variables
            EntityRef                 soulWallEntity;
            BattleSoulWallQComponent* soulWall;
            Transform2D*              soulWallTransform;
            PhysicsCollider2D*        soulWallCollider;

            // create soulwalls
            foreach (BattleSoulWallTemplate soulWallTemplate in soulWallTemplates)
            {
                // create entity
                soulWallEntity = f.Create(soulWallPrototypes[soulWallTemplate.Width - 1]);

                // get components
                soulWall          = f.Unsafe.GetPointer<BattleSoulWallQComponent>(soulWallEntity);
                soulWallTransform = f.Unsafe.GetPointer<Transform2D>(soulWallEntity);
                soulWallCollider  = f.Unsafe.GetPointer<PhysicsCollider2D>(soulWallEntity);

                // set temp variables
                soulWallPosition        = BattleGridManager.GridPositionToWorldPosition(soulWallTemplate.Position);
                soulWallEmotionIndex    = f.RNG->NextInclusive((int)BattleEmotionState.Sadness, (int)BattleEmotionState.Aggression);
                soulWallColliderExtents = soulWallCollider->Shape.Box.Extents;

                // initialize soulwall component
                soulWall->Emotion            = (BattleEmotionState)soulWallEmotionIndex;
                soulWall->Normal             = soulWallNormal;
                soulWall->CollisionMinOffset = soulWallScale * FP._0_50;

                // initialize collider
                soulWallCollider->Shape = Shape2D.CreateBox(
                    soulWallColliderExtents * soulWallScale,
                    new FPVector2((soulWallColliderExtents.X - FP._0_50) * soulWallScale, 0
                ));

                // teleport entity
                soulWallTransform->Teleport(f, soulWallPosition, FP._0);

                // initialize view
                f.Events.SoulWallViewInit(soulWallEntity, soulWallScale, soulWallEmotionIndex, soulWallTemplate.ColorIndex);
            }
        }
    }
}
