/// @file BattlePlayerMovementQSystem.cs
/// <summary>
/// Handles player input, movement and rotations.
/// </summary>
///
/// Gets player's Quantum.Input and updates player's position and rotation depending on player's actions.
/// Handles moving, rotating and teleporting players and all their hitboxes.

using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Input = Quantum.Input;
using Quantum.Collections;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">PlayerMovement <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles player input, movement and rotations.<br/>
    /// </summary>
    [Preserve]
    public unsafe class BattlePlayerMovementQSystem : SystemMainThreadFilter<BattlePlayerMovementQSystem.Filter>
    {
        /// <summary>
        /// Filter for filtering player entities.
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattlePlayerDataQComponent* PlayerData;
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Handles player input, movement and rotations.<br/>
        /// Skips players that have PlayerRef = none.<br/>
        /// Gets player's Quantum Input and calls <see cref="UpdatePlayerMovement(Frame, ref Filter, Input*)">UpdatePlayerMovement</see> method.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.PlayerData->PlayerRef == PlayerRef.None) return;
            Input* input = f.GetPlayerInput(filter.PlayerData->PlayerRef);

            UpdatePlayerMovement(f, ref filter, input);
        }

        /// <summary>
        /// Moves the player towards the specified position while updating the hitbox.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="position">Target world position to move towards.</param>
        /// <param name="maxDelta">Maximum movement delta per frame.</param>
        public static void MoveTowards(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            MoveTowardsNoHitboxUpdate(f, transform, position, maxDelta);
            MoveHitbox(f, playerData, transform);
        }

        /// <summary>
        /// Rotates the player to the specified angle while updating the hitbox.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="radians">Target rotation angle in radians.</param>
        public static void Rotate(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FP radians)
        {
            RotateNoHitboxUpdate(f, transform, radians);
            MoveHitbox(f, playerData, transform);
        }

        /// <summary>
        /// Instantly moves and rotates the player to the specified position and rotation, and teleports the hitbox.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="position">New world position.</param>
        /// <param name="rotation">New rotation in radians.</param>
        public static void Teleport(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP rotation)
        {
            TeleportNoHitboxUpdate(f, transform, position, rotation);
            TeleportHitbox(f, playerData, transform);
        }

        /// <summary>
        /// Private method for moving only player towards the specified position.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="position">Target world position to move towards.</param>
        /// <param name="maxDelta">Maximum movement delta per frame.</param>
        private static void MoveTowardsNoHitboxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            transform->Position = FPVector2.MoveTowards(transform->Position, position, maxDelta);
        }

        /// <summary>
        /// Private method for rotating only player to the specified angle.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="radians">Target rotation angle in radians.</param>
        private static void RotateNoHitboxUpdate(Frame f, Transform2D* transform, FP radians)
        {
            transform->Rotation = radians;
        }

        /// <summary>
        /// Private method for instantly teleporting only player to the specified position and rotation.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="position">New world position.</param>
        /// <param name="rotation">New rotation in radians.</param>
        private static void TeleportNoHitboxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP rotation)
        {
            transform->Teleport(f, position, rotation);
        }

        /// <summary>
        /// Inlined private method for calculating hitbox's position based on player's position, rotation and hitbox's offset from player.
        /// </summary>
        /// <param name="position">Player's position.</param>
        /// <param name="rotation">Player's rotation.</param>
        /// <param name="offset">Offset between player and hitbox.</param>
        /// <returns>FPVector2 of hitbox's position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FPVector2 CalculateHitboxPosition(FPVector2 position, FP rotation, FPVector2 offset) => position + FPVector2.Rotate(offset, rotation);

        /// <summary>
        /// Private method for moving all of player's hitboxes towards the specified position and/or moves them when player rotates.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        private static void MoveHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<BattlePlayerHitboxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (BattlePlayerHitboxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Position = CalculateHitboxPosition(position, rotation, hitBoxLink.Position);
                hitBoxTransform->Rotation = rotation;
            }
        }

        /// <summary>
        /// Private method for instantly teleporting all of player's hitboxes to the specified position and rotation.
        /// </summary>
        /// <param name="f">Current Quantum frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        private static void TeleportHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<BattlePlayerHitboxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (BattlePlayerHitboxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Teleport(f,
                    CalculateHitboxPosition(position, rotation, hitBoxLink.Position),
                    rotation
                );
            }
        }

        /// <summary>
        /// Private helper method for the public <see cref="Update(Frame, ref Filter)">Update</see> method.<br/>
        /// Handles player's movement and rotation.
        /// </summary>
        /// Checks Quantum Input for player's actions.<br/>
        /// When movement action is taken: Checks and updates player's TargetPosition based on input.<br/>
        /// When rotation action is taken: updates player's RotationOffset based on input.<br/>
        /// When rotation action is not taken: updates player's RotationOffset back to zero.<br/>
        /// Always: updates player's position and rotation based on the current TargetPosition and RotationOffset.<br/>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        /// <param name="input">Player's Quantum Input</param>
        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            // constant
            FP rotationSpeed = FP._0_20;

            // unpack filter
            BattlePlayerDataQComponent* playerData = filter.PlayerData;
            Transform2D* transform = filter.Transform;

            // handle movement
            if (input->MouseClick)
            {
                // get player's TargetPosition
                BattleGridPosition targetGridPosition = input->MovementPosition;

                // clamp the TargetPosition inside sidebounds
                targetGridPosition.Col = Mathf.Clamp(targetGridPosition.Col, 0, BattleGridManager.Columns-1);

                // clamp the TargetPosition inside team's playfield for alphateam
                if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        BattleGridManager.TeamAlphaFieldStart + playerData->GridExtendBottom,
                        BattleGridManager.TeamAlphaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // clamp the TargetPosition inside team's playfield for betateam
                else
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        BattleGridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                        BattleGridManager.TeamBetaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // get player's TargetPositions as WorldPosition
                playerData->TargetPosition = BattleGridManager.GridPositionToWorldPosition(targetGridPosition);

                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", playerData->TargetPosition);
            }

            // handle rotation
            {
                if (input->RotateMotion)
                {
                    FP maxAngle = FP.Rad_45;

                    //stop player before rotation
                    playerData->TargetPosition = transform->Position;

                    //rotate to right
                    if (input->RotationDirection > 0 && playerData->RotationOffset < maxAngle)
                    {
                        playerData->RotationOffset += rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", playerData->RotationOffset);
                    }

                    //rotate to left
                    else if (input->RotationDirection < 0 && playerData->RotationOffset > -maxAngle)
                    {
                        playerData->RotationOffset -= rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", playerData->RotationOffset);
                    }
                }

                // return player to 0 rotation when RotateMotion-input ends
                if (!input->RotateMotion && playerData->RotationOffset != 0)
                {
                    if (playerData->RotationOffset > 0)
                        playerData->RotationOffset -= rotationSpeed;

                    else
                        playerData->RotationOffset += rotationSpeed;
                }
            }

            // update position and rotation
            {
                RotateNoHitboxUpdate(f, transform, playerData->RotationBase + playerData->RotationOffset);

                if (transform->Position != playerData->TargetPosition)
                    MoveTowardsNoHitboxUpdate(f, transform, playerData->TargetPosition, playerData->StatSpeed * f.DeltaTime);

                MoveHitbox(f, playerData, transform);
            }
        }
    }
}
