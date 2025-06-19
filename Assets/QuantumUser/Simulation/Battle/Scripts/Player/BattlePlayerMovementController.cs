using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Input = Quantum.Input;
using Quantum.Collections;
using Photon.Deterministic;

using Battle.QSimulation.Game;
using UnityEditor;

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
        /// Checks Quantum Input for player's actions.<br/>
        /// When movement action is taken: Checks and updates player's TargetPosition based on input.<br/>
        /// When rotation action is taken: updates player's RotationOffset based on input.<br/>
        /// When rotation action is not taken: updates player's RotationOffset back to zero.<br/>
        /// Always: updates player's position and rotation based on the current TargetPosition and RotationOffset.<br/>
        /// <param name="f">Current Quantum Frame</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter</a>.</param>
        /// <param name="input">Player's Quantum Input</param>
        public static void UpdateMovement(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, Input* input)
        {
            // constant
            FP rotationSpeed = FP._10;

            FPVector2 positionNext = transform->Position;

            // handle movement
            if (input->MovementInput)
            {
                // get players TargetPosition
                if (input->MovementDirection != FPVector2.Zero)
                {
                    positionNext = transform->Position + FPVector2.ClampMagnitude(input->MovementDirection, FP._1) * playerData->Stats.Speed * f.DeltaTime;
                    if (ClampPosition(playerData, positionNext, out FPVector2 clampedPosition))
                    {
                        positionNext = clampedPosition;
                    }
                    playerData->TargetPosition = positionNext;
                }
                else
                {
                    ClampPosition(playerData, input->MovementPosition, out playerData->TargetPosition);
                    playerData->HasTargetPosition = true;
                }

                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", playerData->TargetPosition);
            }

            if (playerData->HasTargetPosition)
            {
                positionNext = FPVector2.MoveTowards(transform->Position, playerData->TargetPosition, playerData->Stats.Speed * f.DeltaTime);
                if (positionNext == playerData->TargetPosition)
                {
                    playerData->HasTargetPosition = false;
                }
            }

            if (!input->MovementInput && !playerData->HasTargetPosition)
            {
                ClampPosition(playerData, transform->Position, out positionNext);
                playerData->TargetPosition = positionNext;
            }

            // handle rotation
            {
                if (input->RotationInput)
                {
                    //set target angle
                    FP maxAngle = FP.Rad_45 * input->RotationValue;
                    maxAngle = FPMath.Clamp(maxAngle, -FP.Rad_45, FP.Rad_45);

                    //stops player before rotation
                    playerData->TargetPosition = transform->Position;

                    //rotates to left
                    if (maxAngle > playerData->RotationOffset)
                    {
                        playerData->RotationOffset += rotationSpeed * f.DeltaTime;
                        if (playerData->RotationOffset > maxAngle)
                        {
                            playerData->RotationOffset = maxAngle;
                        }
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", playerData->RotationOffset);
                    }

                    //rotates to right
                    else if (maxAngle < playerData->RotationOffset)
                    {
                        playerData->RotationOffset -= rotationSpeed * f.DeltaTime;
                        if (playerData->RotationOffset < maxAngle)
                        {
                            playerData->RotationOffset = maxAngle;
                        }
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", playerData->RotationOffset);
                    }
                }

                // returns player to 0 rotation when RotateMotion-input ends
                if (!input->RotationInput && playerData->RotationOffset != 0)
                {
                    if (playerData->RotationOffset > 0)
                    {
                        playerData->RotationOffset -= rotationSpeed * f.DeltaTime;
                        if (playerData->RotationOffset < 0)
                        {
                            playerData->RotationOffset = 0;
                        }
                    }
                    else
                    {
                        playerData->RotationOffset += rotationSpeed * f.DeltaTime;
                        if (playerData->RotationOffset > 0)
                        {
                            playerData->RotationOffset = 0;
                        }
                    }
                }
            }

            // update position and rotation
            MoveAndRotate(f, playerData, transform, positionNext, playerData->RotationBase + playerData->RotationOffset);
        }

        public static void Move(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position)
        {
            transform->Position = position;
            MoveHitbox(f, playerData, transform);
        }

        public static void Rotate(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FP radians)
        {
            transform->Rotation = radians;
            MoveHitbox(f, playerData, transform);
        }

        public static void MoveAndRotate(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP radians)
        {
            transform->Position = position;
            transform->Rotation = radians;
            MoveHitbox(f, playerData, transform);
        }

        public static void Teleport(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP rotation)
        {
            transform->Teleport(f, position, rotation);
            TeleportHitbox(f, playerData, transform);
        }

        private static void MoveHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            Transform2D* shieldTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxShieldEntity);
            Transform2D* characterTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxCharacterEntity);

            shieldTransform->Position = transform->Position;
            shieldTransform->Rotation = transform->Rotation;
            characterTransform->Position = transform->Position;
            characterTransform->Rotation = transform->Rotation;

            f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->Normal = FPVector2.Rotate(FPVector2.Up, transform->Rotation);
        }

        private static void TeleportHitbox(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform)
        {
            Transform2D* shieldTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxShieldEntity);
            Transform2D* characterTransform = f.Unsafe.GetPointer<Transform2D>(playerData->HitboxCharacterEntity);

            shieldTransform->Teleport(f, transform->Position, transform->Rotation);
            characterTransform->Teleport(f, transform->Position, transform->Rotation);

            f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->Normal = FPVector2.Rotate(FPVector2.Up, transform->Rotation);
        }

        private static bool ClampPosition(BattlePlayerDataQComponent* playerData, BattleGridPosition gridPosition, out FPVector2 clampedPosition)
        {
            BattleGridPosition clampedGridPosition;

            // clamp the TargetPosition inside sidebounds
            clampedGridPosition.Col = Mathf.Clamp(gridPosition.Col, 0, BattleGridManager.Columns - 1);

            // clamp the TargetPosition inside teams playfield for alphateam
            if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
            {
                clampedGridPosition.Row = Mathf.Clamp(
                    gridPosition.Row,
                    BattleGridManager.TeamAlphaFieldStart + playerData->GridExtendBottom,
                    BattleGridManager.TeamAlphaFieldEnd - playerData->GridExtendTop
                );
            }

            // clamp the TargetPosition inside teams playfield for betateam
            else
            {
                clampedGridPosition.Row = Mathf.Clamp(
                    gridPosition.Row,
                    BattleGridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                    BattleGridManager.TeamBetaFieldEnd - playerData->GridExtendTop
                );
            }

            clampedPosition = BattleGridManager.GridPositionToWorldPosition(clampedGridPosition);

            return gridPosition.Col != clampedGridPosition.Col || gridPosition.Row != clampedGridPosition.Row;

        }

        private static bool ClampPosition(BattlePlayerDataQComponent* playerData, FPVector2 position, out FPVector2 clampedPosition)
        {
            BattleGridPosition gridPosition = BattleGridManager.WorldPositionToGridPosition(position);

            return ClampPosition(playerData, gridPosition, out clampedPosition);
        }
    }
}
