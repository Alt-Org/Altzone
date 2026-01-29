/// @file BattlePlayerMovementController.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerMovementController} class
/// which contains the primary method for handling player movement as well as individual helper methods for moving and rotating players.
/// </summary>

// Unity usings
using UnityEngine;
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Handles player movement and rotation
    /// </summary>
    ///
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview)
    ///
    /// Contains the primary @cref{BattlePlayerMovementController,UpdateMovement} method which handles player movement, and is called by BattlePlayerQSystem.<br/>
    /// Also contains individual helper methods for moving and rotating players, which can be used by other scripts.
    [Preserve]
    public static unsafe class BattlePlayerMovementController
    {
        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create(typeof(BattlePlayerMovementController));
        }

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
        /// <param name="playerEntityRef">Reference to player entity.</param>
        /// <param name="transform">Pointer to player's transform component.</param>
        /// <param name="input">Pointer to player's Quantum Input.</param>
        public static void UpdateMovement(Frame f, BattlePlayerDataQComponent* playerData, EntityRef playerEntityRef, Transform2D* transform, Input* input)
        {
            BattlePlayerQSpec spec = BattleQConfig.GetPlayerSpec(f);

            // constant
            FP rotationSpeed = spec.RotationSpeed;

            FPVector2 positionNext = transform->Position;

            //{ handle movement

            // handle movement input
            switch (input->MovementInput)
            {
                case BattleMovementInputType.None:
                    break;

                case BattleMovementInputType.PositionTarget:
                    ClampPosition(playerData, input->MovementPositionTarget, out playerData->TargetPosition);
                    playerData->HasTargetPosition = true;
                    break;

                case BattleMovementInputType.PositionMove:
                    positionNext = FPVector2.MoveTowards(transform->Position, input->MovementPositionMove, playerData->Stats.Speed * f.DeltaTime);
                    if (ClampPosition(playerData, positionNext, out FPVector2 clampedNext))
                    {
                        positionNext = clampedNext;
                    }
                    playerData->TargetPosition = positionNext;
                    break;

                case BattleMovementInputType.Direction:
                    FPVector2 movementDirection = input->MovementDirection * (input->MovementDirectionIsNormalized ? playerData->Stats.Speed : FP._1);
                    positionNext = transform->Position + FPVector2.ClampMagnitude(movementDirection, playerData->Stats.Speed) * f.DeltaTime;
                    if (ClampPosition(playerData, positionNext, out FPVector2 clampedPosition))
                    {
                        positionNext = clampedPosition;
                    }
                    playerData->TargetPosition = positionNext;
                    break;
            }

            // handle target position based movement
            if (playerData->HasTargetPosition)
            {
                positionNext = FPVector2.MoveTowards(transform->Position, playerData->TargetPosition, playerData->Stats.Speed * f.DeltaTime);
                if (positionNext == playerData->TargetPosition)
                {
                    playerData->HasTargetPosition = false;
                }
            }

            // cancel movement if needed
            if (input->RotationInput || (input->MovementInput == BattleMovementInputType.None && !playerData->HasTargetPosition))
            {
                ClampPosition(playerData, transform->Position, out positionNext);
                playerData->TargetPosition = positionNext;
            }

            //} handle movement

            //{ handle rotation

            // handle rotation input
            if (input->RotationInput && !playerData->DisableRotation)
            {
                // set target angle
                FP maxAngle = FP.Rad_45 * input->RotationValue;
                FP maxAllowedAngle = spec.MaxRotationAngleDeg * FP.Deg2Rad;
                maxAngle = FPMath.Clamp(maxAngle, -maxAllowedAngle, maxAllowedAngle);

                // rotates to left
                if (maxAngle > playerData->RotationOffset)
                {
                    playerData->RotationOffset += rotationSpeed * f.DeltaTime;
                    if (playerData->RotationOffset > maxAngle)
                    {
                        playerData->RotationOffset = maxAngle;
                    }
                    s_debugLogger.LogFormat(f, "Leaning left(rotation: {0}", playerData->RotationOffset);
                }

                // rotates to right
                else if (maxAngle < playerData->RotationOffset)
                {
                    playerData->RotationOffset -= rotationSpeed * f.DeltaTime;
                    if (playerData->RotationOffset < maxAngle)
                    {
                        playerData->RotationOffset = maxAngle;
                    }
                    s_debugLogger.LogFormat(f, "Leaning right(rotation: {0}", playerData->RotationOffset);
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

            //} handle rotation

            // update position and rotation
            Move(f, playerData, playerEntityRef, positionNext);
        }

        /// <summary>
        /// Moves and rotates the player to the specified position and angle while updating the hitbox.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="playerEntityRef">Reference to player entity.</param>
        /// <param name="position">World position to move to.</param>
        public static void Move(Frame f, BattlePlayerDataQComponent* playerData, EntityRef playerEntityRef, FPVector2 position)
        {
            BattleEntityManager.MoveCompound(f, playerEntityRef, position, playerData->RotationBase);
            BattleEntityManager.MoveCompound(f, playerData->AttachedShield, position, playerData->RotationBase);
        }

        /// <summary>
        /// Instantly moves and rotates the player to the specified position and rotation, and teleports the hitbox.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="characterTransform">Pointer to the player's transform component.</param>
        /// <param name="shieldTransform">Pointer to the shield's transform component.</param>
        /// <param name="position">New world position.</param>
        /// <param name="rotation">New rotation in radians.</param>
        public static void Teleport(Frame f, BattlePlayerDataQComponent* playerData, EntityRef playerEntityRef, FPVector2 position)
        {
            BattleEntityManager.TeleportCompound(f, playerEntityRef, position, playerData->RotationBase);
            BattleEntityManager.TeleportCompound(f, playerData->AttachedShield, position, playerData->RotationBase);
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>
        /// Clamps the grid position of the player to the playfield of their team.
        /// </summary>
        ///
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="gridPosition">The grid position of the player.</param>
        /// <param name="clampedPosition">The resulting clamped position of the player.</param>
        ///
        /// <returns>True if the position changed from clamping, false if it remained the same.</returns>
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

        /// <summary>
        /// Clamps the world position of the player to the playfield of their team.
        /// </summary>
        ///
        /// <param name="playerData">Pointer to the player's data component.</param>
        /// <param name="position">The world position of the player.</param>
        /// <param name="clampedPosition">The resulting clamped position of the player.</param>
        ///
        /// <returns>True if the position changed from clamping, false if it remained the same.</returns>
        private static bool ClampPosition(BattlePlayerDataQComponent* playerData, FPVector2 position, out FPVector2 clampedPosition)
        {
            BattleGridPosition gridPosition = BattleGridManager.WorldPositionToGridPosition(position);

            return ClampPosition(playerData, gridPosition, out clampedPosition);
        }
    }
}
