using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerData* PlayerData;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.PlayerData->PlayerRef == PlayerRef.None) return;
            Input* input = f.GetPlayerInput(filter.PlayerData->PlayerRef);

            UpdatePlayerMovement(f, ref filter, input);
        }

        public static void MoveTowards(Frame f, PlayerData* playerData, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            MoveTowardsNoHitboxUpdate(f, transform, position, maxDelta);
            MoveHitbox(f, playerData, transform);
        }

        public static void Rotate(Frame f, PlayerData* playerData, Transform2D* transform, FP radians)
        {
            RotateNoHitboxUpdate(f, transform, radians);
            MoveHitbox(f, playerData, transform);
        }

        public static void Teleport(Frame f, PlayerData* playerData, Transform2D* transform, FPVector2 position, FP rotation)
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

        private static void MoveHitbox(Frame f, PlayerData* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<PlayerHitboxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitboxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Position = CalculateHitboxPosition(position, rotation, hitBoxLink.Position);
                hitBoxTransform->Rotation = rotation;
            }
        }

        private static void TeleportHitbox(Frame f, PlayerData* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<PlayerHitboxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitboxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Teleport(f,
                    CalculateHitboxPosition(position, rotation, hitBoxLink.Position),
                    rotation
                );
            }
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            // constant
            FP rotationSpeed = FP._0_20;

            // unpack filter
            PlayerData* playerData = filter.PlayerData;
            Transform2D* transform = filter.Transform;

            // handle movement
            if (input->MouseClick)
            {
                // get players TargetPosition
                GridPosition targetGridPosition = input->MovementPosition;

                // clamp the TargetPosition inside sidebounds
                targetGridPosition.Col = Mathf.Clamp(targetGridPosition.Col, 0, GridManager.Columns-1);

                // clamp the TargetPosition inside teams playfield for alphateam
                if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        GridManager.TeamAlphaFieldStart + playerData->GridExtendBottom,
                        GridManager.TeamAlphaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // clamp the TargetPosition inside teams playfield for betateam
                else
                {
                    targetGridPosition.Row = Mathf.Clamp(
                        targetGridPosition.Row,
                        GridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                        GridManager.TeamBetaFieldEnd   - playerData->GridExtendTop
                    );
                }

                // get players TargetPositions as WorldPosition
                playerData->TargetPosition = GridManager.GridPositionToWorldPosition(targetGridPosition);

                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", playerData->TargetPosition);
            }

            // handle rotation
            {
                if (input->RotateMotion)
                {
                    FP maxAngle = FP.Rad_45;

                    //stops player before rotation
                    playerData->TargetPosition = transform->Position;

                    //rotates to right
                    if (input->RotationDirection > 0 && playerData->RotationOffset < maxAngle)
                    {
                        playerData->RotationOffset += rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", playerData->RotationOffset);
                    }

                    //rotates to left
                    else if (input->RotationDirection < 0 && playerData->RotationOffset > -maxAngle)
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
                    MoveTowardsNoHitboxUpdate(f, transform, playerData->TargetPosition, playerData->StatSpeed * f.DeltaTime);

                MoveHitbox(f, playerData, transform);
            }
        }
    }
}
