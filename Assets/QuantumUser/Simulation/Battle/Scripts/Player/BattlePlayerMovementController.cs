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
            FP rotationSpeed = 4;

            // handle movement
            if (input->MovementInput)
            {
                // get players TargetPosition
                BattleGridPosition targetGridPosition;
                if (input->MovementDirection != FPVector2.Zero)
                {
                    Debug.LogWarning(input->MovementDirection);
                    FPVector2 targetWorldPosition = transform->Position + input->MovementDirection;
                    targetGridPosition = new BattleGridPosition()
                    {
                        Row = BattleGridManager.WorldYPositionToGridRow(targetWorldPosition.Y),
                        Col = BattleGridManager.WorldXPositionToGridCol(targetWorldPosition.X)
                    };
                }
                else
                {
                    targetGridPosition = input->MovementPosition;
                }

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
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", playerData->RotationOffset);
                    }

                    //rotates to right
                    else if (maxAngle < playerData->RotationOffset)
                    {
                        playerData->RotationOffset -= rotationSpeed * f.DeltaTime;
                        if (playerData->RotationOffset < maxAngle)
                        {
                            playerData->RotationOffset = maxAngle;
                        }
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", playerData->RotationOffset);
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
            {
                RotateNoHitboxUpdate(f, transform, playerData->RotationBase + playerData->RotationOffset);

                if (transform->Position != playerData->TargetPosition)
                    MoveTowardsNoHitboxUpdate(f, transform, playerData->TargetPosition, playerData->Stats.Speed * f.DeltaTime);

                MoveHitbox(f, playerData, transform);
            }
        }

        public static void MoveTowards(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            MoveTowardsNoHitboxUpdate(f, transform, position, maxDelta);
            MoveHitbox(f, playerData, transform);
        }

        public static void Rotate(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FP radians)
        {
            RotateNoHitboxUpdate(f, transform, radians);
            MoveHitbox(f, playerData, transform);
        }

        public static void Teleport(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* transform, FPVector2 position, FP rotation)
        {
            TeleportNoHitboxUpdate(f, transform, position, rotation);
            TeleportHitbox(f, playerData, transform);
        }

        private static void MoveTowardsNoHitboxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            transform->Position = FPVector2.MoveTowards(transform->Position, position, maxDelta);
        }

        private static void RotateNoHitboxUpdate(Frame f, Transform2D* transform, FP radians)
        {
            transform->Rotation = radians;
        }

        private static void TeleportNoHitboxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP rotation)
        {
            transform->Teleport(f, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FPVector2 CalculateHitboxPosition(FPVector2 position, FP rotation, FPVector2 offset) => position + FPVector2.Rotate(offset, rotation);

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
