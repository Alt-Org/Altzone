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
    /// Handles player input, movement and rotations.<br/>
    /// </summary>
    [Preserve]
    public static unsafe class BattlePlayerMovementController
    {
        /// <summary>
        /// Handles player's movement and rotation.
        /// </summary>
        ///
        /// Checks Quantum Input for player's actions.<br/>
        /// When movement action is taken: Checks and updates player's TargetPosition based on input.<br/>
        /// When rotation action is taken: updates player's RotationOffset based on input.<br/>
        /// When rotation action is not taken: updates player's RotationOffset back to zero.<br/>
        /// Always: updates player's position and rotation based on the current TargetPosition and RotationOffset.<br/>
        ///
        /// <param name="f">Current %Quantum %Frame.</param>
        /// <param name="playerData">Pointer to player's BattlePlayerDataQComponent.</param>
        /// <param name="transform">Pointer to player's transform component.</param>
        /// <param name="input">Pointer to player's Quantum Input.</param>
        public static void UpdateMovement(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, Input* input)
        {
            // constant
            FP rotationSpeed = FP._0_20;

            // handle movement
            if (input->MouseClick)
            {
                // get players TargetPosition
                BattleGridPosition targetGridPosition = input->MovementPosition;

                // clamp the TargetPosition inside sidebounds
                targetGridPosition.Col = Mathf.Clamp(targetGridPosition.Col, 0, BattleGridManager.Columns - 1);

                // clamp the TargetPosition inside teams playfield for alphateam
                if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        BattleGridManager.TeamAlphaFieldStart + playerData->GridExtendBottom,
                        BattleGridManager.TeamAlphaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // clamp the TargetPosition inside teams playfield for betateam
                else
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        BattleGridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                        BattleGridManager.TeamBetaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // get players TargetPositions as WorldPosition
                playerData->TargetPosition = BattleGridManager.GridPositionToWorldPosition(targetGridPosition);

                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", playerData->TargetPosition);
            }

            // handle rotation
            {
                if (input->RotateMotion)
                {
                    //set target angle
                    FP maxAngle;
#if PLATFORM_ANDROID
                    maxAngle = FP.Rad_45 * (FPMath.Abs(input->RotationValue) / (FP)45);
#else
                    maxAngle = FP.Rad_45 * (FPMath.Abs(input->RotationValue) / (FP)360);
#endif
                    maxAngle = FPMath.Clamp(maxAngle, FP._0, FP.Rad_45);

                    //stops player before rotation
                    playerData->TargetPosition = transform->Position;

                    //rotates to right
                    if (input->RotationValue > 0 && playerData->RotationOffset < maxAngle)
                    {
                        playerData->RotationOffset += rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", playerData->RotationOffset);
                    }

                    //rotates to left
                    else if (input->RotationValue < 0 && playerData->RotationOffset > -maxAngle)
                    {
                        playerData->RotationOffset -= rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", playerData->RotationOffset);
                    }
                }

                // returns player to 0 rotation when RotateMotion-input ends
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
                    MoveTowardsNoHitboxUpdate(f, transform, playerData->TargetPosition, playerData->Stats.Speed * f.DeltaTime);

                MoveHitbox(f, playerData, transform);
            }
        }

        /// <summary>
        /// Moves the player towards the specified position while updating the hitbox.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
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
        /// <param name="f">Current simulation frame.</param>
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
        /// <param name="f">Current simulation frame.</param>
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
        /// <param name="f">Current simulation frame.</param>
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
        /// <param name="f">Current simulation frame.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        /// <param name="radians">Target rotation angle in radians.</param>
        private static void RotateNoHitboxUpdate(Frame f, Transform2D* transform, FP radians)
        {
            transform->Rotation = radians;
        }

        /// <summary>
        /// Private method for instantly teleporting only player to the specified position and rotation.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
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
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        private static void MoveHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            //if (!f.TryResolveList(playerData->HitboxListAll, out QList<BattlePlayerHitboxLink> hitboxListAll)) return;

            Transform2D* shieldTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxShieldEntity);
            Transform2D* characterTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxCharacterEntity);

            shieldTransform->Position = transform->Position;
            shieldTransform->Rotation = transform->Rotation;
            characterTransform->Position = transform->Position;
            characterTransform->Rotation = transform->Rotation;

            f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->Normal = FPVector2.Rotate(FPVector2.Up, transform->Rotation);
        }

        /// <summary>
        /// Private method for instantly teleporting all of player's hitboxes to the specified position and rotation.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="transform">Pointer to the player's transform component.</param>
        private static void TeleportHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            //if (!f.TryResolveList(playerData->HitboxListAll, out QList<BattlePlayerHitboxLink> hitboxListAll)) return;

            Transform2D* shieldTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxShieldEntity);
            Transform2D* characterTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxCharacterEntity);

            shieldTransform->Teleport(f, transform->Position, transform->Rotation);
            characterTransform->Teleport(f, transform->Position, transform->Rotation);

            f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->Normal = FPVector2.Rotate(FPVector2.Up, transform->Rotation);
        }
    }
}
