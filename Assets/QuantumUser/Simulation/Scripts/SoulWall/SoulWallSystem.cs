using UnityEngine;
using UnityEngine.Scripting;
using Photon.Deterministic;

using Quantum.QuantumUser.Simulation.Projectile;

namespace Quantum.QuantumUser.Simulation.SoulWall
{
    [Preserve]
    public unsafe class SoulWallSystem : SystemSignalsOnly, ISignalOnTriggerProjectileHitSoulWall
    {
        /// <summary>
        /// Creates soulwalls based on BattleArena and SoulWall Specs during map creation phase.<br/>
        /// (this method should only be called once by GameControlSystem during the map creation)
        /// </summary>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="battleArenaSpec">The BattleArenaSpec</param>
        /// <param name="soulWallSpec">The SoulWallSpec</param>
        public static void CreateSoulWalls(Frame f, BattleArenaSpec battleArenaSpec, SoulWallSpec soulWallSpec)
        {
            // create soulwall entities on both sides of the arena
            CreateSoulWalls(f, BattleTeamNumber.TeamAlpha, battleArenaSpec.SoulWallTeamAlphaTemplates, soulWallSpec.SoulWallPrototypes);
            CreateSoulWalls(f, BattleTeamNumber.TeamBeta,  battleArenaSpec.SoulWallTeamBetaTemplates,  soulWallSpec.SoulWallPrototypes);
        }

        public void OnTriggerProjectileHitSoulWall(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.SoulWall* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");

            if (ProjectileSystem.IsCollisionFlagSet(f, projectile, ProjectileCollisionFlags.SoulWall)) return;

            // Destroy the SoulWall entity
            f.Events.PlaySoundEvent(SoundEffect.WallBroken);
            f.Destroy(soulWallEntity);

            ProjectileSystem.SetCollisionFlag(f, projectile, ProjectileCollisionFlags.SoulWall);
        }

        /// <summary>
        /// Private helper method (for the public <see cref="CreateSoulWalls(Frame, BattleArenaSpec SoulWallSpec)">CreateSoulWalls</see>) that creates soulwalls on one side of the arena.
        /// </summary>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="teamNumber">The teamNumber of the team whose side is being created</param>
        /// <param name="soulWallTemplates">An array of soulwall templates that are used to create the soulwalls </param>
        /// <param name="soulWallPrototypes">An array of entityPrototypes that can be created</param>
        private static void CreateSoulWalls(Frame f, BattleTeamNumber teamNumber, SoulWallTemplate[] soulWallTemplates, AssetRef<EntityPrototype>[] soulWallPrototypes)
        {
            // soulwall temp variables
            FPVector2 soulWallPosition;
            int       soulWallEmotionIndex;
            FP        soulWallScale;
            FPVector2 soulWallNormal;
            FPVector2 soulWallColliderExtents;

            // set all soulwall common temp variables (used for all soulwalls on this side)
            soulWallScale  = GridManager.GridScaleFactor;
            soulWallNormal = new FPVector2(0, teamNumber == BattleTeamNumber.TeamAlpha ? FP._1 : FP.Minus_1);

            // soulwall variables
            EntityRef          soulWallEntity;
            Quantum.SoulWall*  soulWall;
            Transform2D*       soulWallTransform;
            PhysicsCollider2D* soulWallCollider;

            // create soulwalls
            foreach (SoulWallTemplate soulWallTemplate in soulWallTemplates)
            {
                // create entity
                soulWallEntity    = f.Create(soulWallPrototypes[soulWallTemplate.WidthType-1]);

                // get components
                soulWall          = f.Unsafe.GetPointer<Quantum.SoulWall>(soulWallEntity);
                soulWallTransform = f.Unsafe.GetPointer<Transform2D>(soulWallEntity);
                soulWallCollider  = f.Unsafe.GetPointer<PhysicsCollider2D>(soulWallEntity);

                // set temp variables
                soulWallPosition        = GridManager.GridPositionToWorldPosition(soulWallTemplate.Position);
                soulWallEmotionIndex    = f.RNG->NextInclusive((int)EmotionState.Sadness,(int)EmotionState.Aggression);
                soulWallColliderExtents = soulWallCollider->Shape.Box.Extents;

                // initialize soulwall component
                soulWall->Emotion            = (EmotionState)soulWallEmotionIndex;
                soulWall->Normal             = soulWallNormal;
                soulWall->CollisionMinOffset = soulWallScale * FP._0_50;

                // initialize collider
                soulWallCollider->Shape = Shape2D.CreateBox(
                    soulWallColliderExtents * soulWallScale,
                    new FPVector2(
                        ( soulWallColliderExtents.X - FP._0_50) * soulWallScale,
                        (-soulWallColliderExtents.Y + FP._0_50) * soulWallScale
                    )
                );

                // teleport entity
                soulWallTransform->Teleport(f, soulWallPosition, FP._0);

                // initialize view
                f.Events.SoulWallViewInit(soulWallEntity, soulWallScale, soulWallEmotionIndex, soulWallTemplate.ColorIndex);
            }
        }
    }
}
