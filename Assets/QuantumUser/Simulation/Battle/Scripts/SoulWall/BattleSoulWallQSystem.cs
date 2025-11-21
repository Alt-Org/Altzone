/// @file BattleSoulWallQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.SoulWall,BattleSoulWallQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles creating and destroying SoulWalls.
/// </summary>

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Projectile;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.SoulWall
{
    /// <summary>
    /// <span class="brief-h">%SoulWall <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles creating SoulWalls and reacting to the projectile colliding with them.
    /// </summary>
    ///
    /// This system creates SoulWalls based on BattleArena and SoulWall Specs when GameControlSystem calls CreateSoulWalls method during the map creation.<br/>
    /// Also destroys SoulWall segment when BattleCollisionQSystem calls the collision method.
    [Preserve]
    public unsafe class BattleSoulWallQSystem : SystemSignalsOnly
    {
        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create<BattleSoulWallQSystem>();
        }

        /// <summary>
        /// Creates soulwalls based on BattleArena and SoulWall Specs during map creation phase.
        /// @warning
        /// This method should only be called once by GameControlSystem during the map creation
        /// </summary>
        ///
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
        /// Called by BattleCollisionQSystem. Destroys the soulwall entity that was hit and sends forward the appropriate event that spawns a lightray if the soul wall hit was in the last row.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="soulWallCollisionData">Collision data related to the soul wall.</param>
        public static void OnProjectileHitSoulWall(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.SoulWallCollisionData* soulWallCollisionData)
        {
            if (projectileCollisionData->Projectile->IsHeld) return;
            s_debugLogger.Log(f, "Soul wall hit");

            if (soulWallCollisionData->SoulWall->Row == BattleSoulWallRow.Last)
            {
                FP battleLightrayRotation = 0;
                BattleLightrayColor battleLightrayColor = BattleLightrayColor.Red;
                BattleLightraySize battleLightraySize = (BattleLightraySize)f.RNG->NextInclusive(0, 2);

                switch (soulWallCollisionData->SoulWall->Team)
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

                Transform2D* soulWallTransform = f.Unsafe.GetPointer<Transform2D>(projectileCollisionData->OtherEntity);

                f.Events.BattleLastRowWallDestroyed(soulWallCollisionData->SoulWall->WallNumber, soulWallCollisionData->SoulWall->Team, battleLightrayRotation, battleLightrayColor, battleLightraySize);
            }

            // Destroy the SoulWall entity
            f.Events.BattlePlaySoundFX(BattleSoundFX.WallBroken);
            f.Destroy(projectileCollisionData->OtherEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectileCollisionData->Projectile, BattleProjectileCollisionFlags.SoulWall);
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>
        /// Private helper method (for the public <see cref="CreateSoulWalls(Frame, BattleArenaQSpec, BattleSoulWallQSpec)">CreateSoulWalls</see>) that creates soulwalls on one side of the arena.
        /// </summary>
        ///
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

                s_debugLogger.LogFormat(f, "SoulWall created\n" +
                                        "Team:               {0}\n" +
                                        "Emotion:            {1}\n" +
                                        "Row:                {2}\n" +
                                        "Normal:             {3}\n" +
                                        "CollisionMinOffset: {4}\n" +
                                        "WallNumber:         {5}",
                                        soulWall->Team,
                                        soulWall->Emotion,
                                        soulWall->Row,
                                        soulWall->Normal,
                                        soulWall->CollisionMinOffset,
                                        soulWall->WallNumber
                                        );

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
