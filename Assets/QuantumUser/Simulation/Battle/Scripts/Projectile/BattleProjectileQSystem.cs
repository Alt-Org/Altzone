using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// Projectile <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System</a>.<br/>
    /// Handles Projectile logic.
    /// </summary>
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnProjectileHitArenaBorder, ISignalBattleOnProjectileHitPlayerHitbox
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattleProjectileQComponent* Projectile;
        }

        /// <summary>
        /// Checks if a specific collision flag is currently set for the projectile.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="flag">Collision flag to check.</param>
        /// <returns>True if the flag is set; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollisionFlagSet(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag) => projectile->CollisionFlags[f.Number % 2].IsFlagSet(flag);

        /// <summary>
        /// Sets a specific collision flag for the current frame.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="flag">Collision flag to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollisionFlag(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag)
        {
            BattleProjectileCollisionFlags flags = projectile->CollisionFlags[f.Number % 2];
            projectile->CollisionFlags[f.Number % 2] = flags.SetFlag(flag);
        }

        /// <summary>
        /// Launches the projectile and sets properties based on the <see cref="Battle.QSimulation.Projectile.BattleProjectileQSpec">Projectile spec.</see>
        /// @warning This method should only be called by Quantum. 
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            BattleProjectileQComponent* projectile = filter.Projectile;
            Transform2D* transform = filter.Transform;

            if (!projectile->IsLaunched)
            {
                // retrieve the projectile speed from the spec
                BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

                // set the projectile speed and direction
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));

                // pick random EmotionState for projectile
                projectile->Emotion = (BattleEmotionState)f.RNG->NextInclusive((int)BattleEmotionState.Sadness,(int)BattleEmotionState.Aggression);
                f.Events.BattleChangeEmotionState(projectile->Emotion);

                // reset CollisionFlags for this frame
                projectile->CollisionFlags[(f.Number) % 2] = 0;

                // set the IsLaunched field to true to ensure it's launched only once
                projectile->IsLaunched = true;

                Debug.Log("Projectile Launched");
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        /// <summary>
        /// Handles behavior when the projectile hits a soul wall.
        /// Updates projectile emotion and applies bounce logic.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="soulWall">Pointer to the SoulWall component.</param>
        /// <param name="soulWallEntity">EntityRef of the SoulWall.</param>
        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            ProjectileDirectionUpdate(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);

            // change projectile's emotion to soulwall's emotion
            projectile->Emotion = soulWall->Emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        /// <summary>
        /// Handles behavior when the projectile hits the arena border.
        /// Applies bounce logic based on border surface normal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="arenaBorder">Pointer to the ArenaBorder component.</param>
        /// <param name="arenaBorderEntity">EntityRef of the ArenaBorder.</param>
        public void BattleOnProjectileHitArenaBorder(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileDirectionUpdate(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        /// <summary>
        /// Handles behavior when the projectile hits a player hitbox.
        /// Applies bounce logic based on surface normal of the hitbox.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="playerHitbox">Pointer to the PlayerHitbox component.</param>
        /// <param name="playerEntity">EntityRef of the player hitbox.</param>
        public void BattleOnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return;
            ProjectileDirectionUpdate(f, projectile,  projectileEntity, playerEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset, playerHitbox->CollisionType);
        }

        private void ProjectileDirectionUpdate(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset, BattlePlayerCollisionType collisionType = BattlePlayerCollisionType.Reflect)
        {
            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            // calculate how far off from other entity's position is the projectile supposed to hit it's surface
            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            // set new projectile direction
            if      (collisionType == BattlePlayerCollisionType.Reflect)  projectile->Direction = FPVector2.Reflect(projectile->Direction, normal);
            else if (collisionType == BattlePlayerCollisionType.Override) projectile->Direction = normal;

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }
    }
}
