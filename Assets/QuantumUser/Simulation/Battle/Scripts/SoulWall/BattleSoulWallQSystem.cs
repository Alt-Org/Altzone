/// @file BattleSoulWallQSystem.cs
/// <summary>
/// Handles creating and destroying SoulWalls.
/// </summary>
///
/// This system creates SoulWalls based on BattleArena and SoulWall Specs when GameControlSystem calls CreateSoulWalls method during the map creation.<br/>
/// Also listens for ISignalBattleOnProjectileHitSoulWall signals and upon receiving them destroys SoulWall segments.

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

using Battle.QSimulation.Projectile;
using Battle.QSimulation.Game;
using Photon.Deterministic;

namespace Battle.QSimulation.SoulWall
{
    /// <summary>
    /// <span class="brief-h">%SoulWall <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles creating SoulWalls and reacting to the projectile colliding with them.
    /// </summary>
    [Preserve]
    public unsafe class BattleSoulWallQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitSoulWall
    {
        /// <summary>
        /// Creates soulwalls based on BattleArena and SoulWall Specs during map creation phase.
        /// @warning this method should only be called once by GameControlSystem during the map creation
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="battleArenaSpec">The BattleArenaSpec.</param>
        /// <param name="soulWallSpec">The SoulWallSpec.</param>
        public static void CreateSoulWalls(Frame f, BattleArenaQSpec battleArenaSpec, BattleSoulWallQSpec soulWallSpec)
        {
            // create soulwall entities on both sides of the arena
            CreateSoulWalls(f, BattleTeamNumber.TeamAlpha, battleArenaSpec.SoulWallTeamAlphaTemplates, soulWallSpec.SoulWallPrototypes);
            CreateSoulWalls(f, BattleTeamNumber.TeamBeta,  battleArenaSpec.SoulWallTeamBetaTemplates,  soulWallSpec.SoulWallPrototypes);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitSoulWall">ISignalBattleOnProjectileHitSoulWall</see> is sent.</span><br/>
        /// Signal handler for when a projectile hits a SoulWall. Handles destroying the SoulWall Entity.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="soulWall">Pointer to the SoulWall component.</param>
        /// <param name="soulWallEntity">EntityRef of the SoulWall.</param>
        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");

            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.SoulWall)) return;

            if (soulWall->Row == BattleSoulWallRow.Last)
            {
                FP battleLightrayRotation = 0;
                BattleLightrayColor battleLightrayColor = BattleLightrayColor.Red;
                BattleLightraySize battleLightraySize = (BattleLightraySize)f.RNG->NextInclusive(0, 2);

                switch (soulWall->Team)
                {
                    case BattleTeamNumber.TeamAlpha:
                        battleLightrayRotation = f.RNG->NextInclusive(-50, 50);
                        battleLightrayColor = BattleLightrayColor.Red;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        battleLightrayRotation = f.RNG->NextInclusive(130, 220);
                        battleLightrayColor = BattleLightrayColor.Blue;
                        break;
                }

                Transform2D* soulWallTransform = f.Unsafe.GetPointer<Transform2D>(soulWallEntity);

                f.Events.BattleLastRowWallDestroyed(soulWall->WallNumber, soulWall->Team, battleLightrayRotation, battleLightrayColor, battleLightraySize);
            }

            // Destroy the SoulWall entity
            f.Events.BattlePlaySoundFX(BattleSoundFX.WallBroken);
            f.Destroy(soulWallEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.SoulWall);
        }

        /// <summary>
        /// Private helper method (for the public <see cref="CreateSoulWalls(Frame, BattleArenaQSpec, BattleSoulWallQSpec)">CreateSoulWalls</see>) that creates soulwalls on one side of the arena.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="teamNumber">The teamNumber of the team whose side is being created.</param>
        /// <param name="soulWallTemplates">An array of soulwall templates that are used to create the soulwalls.</param>
        /// <param name="soulWallPrototypes">An array of entityPrototypes that can be created.</param>
        private static void CreateSoulWalls(Frame f, BattleTeamNumber teamNumber, BattleSoulWallTemplate[] soulWallTemplates, AssetRef<EntityPrototype>[] soulWallPrototypes)
        {
            // soulwall temp variables
            FPVector2         soulWallPosition;
            int               soulWallEmotionIndex;
            BattleSoulWallRow soulWallRow = BattleSoulWallRow.First;
            FP                soulWallScale;
            FPVector2         soulWallNormal;
            FPVector2         soulWallColliderExtents;

            // set all soulwall common temp variables (used for all soulwalls on this side)
            soulWallScale  = BattleGridManager.GridScaleFactor;
            soulWallNormal = new FPVector2(0, teamNumber == BattleTeamNumber.TeamAlpha ? FP._1 : FP.Minus_1);

            // soulwall variables
            EntityRef                 soulWallEntity;
            BattleSoulWallQComponent* soulWall;
            Transform2D*              soulWallTransform;
            PhysicsCollider2D*        soulWallCollider;

            // helper variable to assign row positions
            int counter = 0;

            // create soulwalls
            foreach (BattleSoulWallTemplate soulWallTemplate in soulWallTemplates)
            {
                // create entity
                soulWallEntity    = f.Create(soulWallPrototypes[soulWallTemplate.WidthType - 1]);

                // get components
                soulWall          = f.Unsafe.GetPointer<BattleSoulWallQComponent>(soulWallEntity);
                soulWallTransform = f.Unsafe.GetPointer<Transform2D>(soulWallEntity);
                soulWallCollider  = f.Unsafe.GetPointer<PhysicsCollider2D>(soulWallEntity);

                //{ set temp variables

                soulWallPosition        = BattleGridManager.GridPositionToWorldPosition(soulWallTemplate.Position);
                soulWallEmotionIndex    = f.RNG->NextInclusive((int)BattleEmotionState.Sadness, (int)BattleEmotionState.Aggression);
                soulWallColliderExtents = soulWallCollider->Shape.Box.Extents;

                if (soulWallTemplate.Position.Row == 5 || soulWallTemplate.Position.Row == BattleGridManager.Rows - 5)
                {
                    soulWallRow = BattleSoulWallRow.First;
                }
                else if (soulWallTemplate.Position.Row == 3 || soulWallTemplate.Position.Row == BattleGridManager.Rows - 3)
                {
                    soulWallRow = BattleSoulWallRow.Middle;
                }
                else if(soulWallTemplate.Position.Row == 1 || soulWallTemplate.Position.Row == BattleGridManager.Rows - 1)
                {
                    soulWallRow = BattleSoulWallRow.Last;
                }

                //} set temp variables

                // initialize soulwall component
                soulWall->Team               = teamNumber;
                soulWall->Emotion            = (BattleEmotionState)soulWallEmotionIndex;
                soulWall->Row                = soulWallRow;
                soulWall->Normal             = soulWallNormal;
                soulWall->CollisionMinOffset = soulWallScale * FP._0_50;
                soulWall->WallNumber         = counter;

                // increment helper counter
                counter++;

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
                if (soulWallRow == BattleSoulWallRow.Last)
                {
                    f.Events.BattleStoneCharacterPieceViewInit(soulWall->WallNumber, soulWall->Team, soulWallEmotionIndex);
                }
                else
                {
                    f.Events.BattleSoulWallViewInit(soulWallEntity, soulWallScale, soulWallEmotionIndex, soulWallTemplate.ColorIndex);
                }
            }
        }
    }
}
