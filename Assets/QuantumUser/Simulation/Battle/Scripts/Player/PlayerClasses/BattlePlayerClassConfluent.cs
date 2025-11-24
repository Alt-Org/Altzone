/// @file BattlePlayerClassConfluent.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassConfluent} class which contains the logic for Confluent class characters.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class containing code overriding shield/projectile collisions to have the projectile always reflect off of the character's shield based on a calculated normal.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 600 Confluent}](#page-concepts-player-class-600-confluent) for more info.}
    public class BattlePlayerClassConfluent : BattlePlayerClassBase
    {
        /// <summary>The BattlePlayerCharacterClass this class is for.</summary>
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Confluent;

        /// <summary>
        /// Called by BattlePlayerClassManager. Reflects the projectile off of the characters hitbox based on a normal calculated from the characters center to the projectiles position.
        /// Also handles love projectile logic, as it is skipped in BattleProjectileQSystem due to the hitbox collision type being set to none.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            if (!shieldCollisionData->PlayerShieldHitbox->IsActive) return;
            if (projectileCollisionData->Projectile->EmotionCurrent == BattleEmotionState.Love) return;
            if (shieldCollisionData->IsLoveProjectileCollision) return;

            FPVector2 normal = f.Unsafe.GetPointer<Transform2D>(projectileCollisionData->ProjectileEntity)->Position - f.Unsafe.GetPointer<Transform2D>(shieldCollisionData->PlayerShieldHitbox->PlayerEntity)->Position;
            FPVector2 direction = FPVector2.Reflect(projectileCollisionData->Projectile->Direction, normal).Normalized;

            BattleProjectileQSystem.HandleIntersection(f, projectileCollisionData->Projectile, projectileCollisionData->ProjectileEntity, projectileCollisionData->OtherEntity, normal, shieldCollisionData->PlayerShieldHitbox->CollisionMinOffset);
            BattleProjectileQSystem.UpdateVelocity(f, projectileCollisionData->Projectile, direction, BattleProjectileQSystem.SpeedChange.Increment);
        }
    }
}
