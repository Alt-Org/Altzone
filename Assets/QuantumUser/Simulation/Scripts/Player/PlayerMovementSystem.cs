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
            MoveTowardsNoHitBoxUpdate(f, transform, position, maxDelta);
            MoveHitBox(f, playerData, transform);
        }

        public static void Rotate(Frame f, PlayerData* playerData, Transform2D* transform, FP radians)
        {
            RotateNoHitBoxUpdate(f, transform, radians);
            MoveHitBox(f, playerData, transform);
        }

        public static void Teleport(Frame f, PlayerData* playerData, Transform2D* transform, FPVector2 position, FP rotation)
        {
            TeleportNoHitBoxUpdate(f, transform, position, rotation);
            TeleportHitBox(f, playerData, transform);
        }

        private static void MoveTowardsNoHitBoxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP maxDelta)
        {
            transform->Position = FPVector2.MoveTowards(transform->Position, position, maxDelta);
        }

        private static void RotateNoHitBoxUpdate(Frame f, Transform2D* transform, FP radians)
        {
            transform->Rotation = radians;
        }

        private static void TeleportNoHitBoxUpdate(Frame f, Transform2D* transform, FPVector2 position, FP rotation)
        {
            transform->Teleport(f, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FPVector2 CalculateHitBoxPosition(FPVector2 position, FP rotation, FPVector2 offset) => position + FPVector2.Rotate(offset, rotation);

        private static void MoveHitBox(Frame f, PlayerData* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<PlayerHitBoxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitBoxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Position = CalculateHitBoxPosition(position, rotation, hitBoxLink.Position);
                hitBoxTransform->Rotation = rotation;
            }
        }

        private static void TeleportHitBox(Frame f, PlayerData* playerData, Transform2D* transform)
        {
            if (!f.TryResolveList(playerData->HitboxListAll, out QList<PlayerHitBoxLink> hitboxListAll)) return;

            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitBoxLink hitBoxLink in hitboxListAll)
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Teleport(f,
                    CalculateHitBoxPosition(position, rotation, hitBoxLink.Position),
                    rotation
                );
            }
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            FP rotationSpeed = FP._0_20;

            // unpack filter
            PlayerData* playerData = filter.PlayerData;
            Transform2D* transform = filter.Transform;

            // handle movement
            if (input->MouseClick)
            {
                playerData->TargetPosition = GridManager.GridPositionToWorldPosition(input->MovementPosition);
                //checks if player is allowed to move to that side of the arena
                if (((playerData->TeamNumber == BattleTeamNumber.TeamAlpha) && playerData->TargetPosition.Y > 0)
                    || ((playerData->TeamNumber == BattleTeamNumber.TeamBeta) && playerData->TargetPosition.X < 0))
                {
                    playerData->TargetPosition.Y = 0;
                }
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
                RotateNoHitBoxUpdate(f, transform, playerData->RotationBase + playerData->RotationOffset);

                if (transform->Position != playerData->TargetPosition)
                    MoveTowardsNoHitBoxUpdate(f, transform, playerData->TargetPosition, playerData->StatSpeed * f.DeltaTime);

                MoveHitBox(f, playerData, transform);
            }
        }
    }
}
