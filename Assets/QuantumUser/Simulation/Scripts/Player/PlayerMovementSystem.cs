using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Photon.Deterministic;

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
            if (filter.PlayerData->Player == PlayerRef.None) return;
            Input* input = f.GetPlayerInput(filter.PlayerData->Player);

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
            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitBoxLink hitBoxLink in f.ResolveList(playerData->PlayerHitboxList))
            {
                hitBoxTransform = f.Unsafe.GetPointer<Transform2D>(hitBoxLink.Entity);

                hitBoxTransform->Position = CalculateHitBoxPosition(position, rotation, hitBoxLink.Position);
                hitBoxTransform->Rotation = rotation;
            }
        }

        private static void TeleportHitBox(Frame f, PlayerData* playerData, Transform2D* transform)
        {
            FPVector2 position = transform->Position;
            FP rotation = transform->Rotation;

            Transform2D* hitBoxTransform;
            foreach (PlayerHitBoxLink hitBoxLink in f.ResolveList(playerData->PlayerHitboxList))
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

            if (input->MouseClick)
            {
                filter.PlayerData->TargetPosition = GridManager.GridPositionToWorldPosition(input->MovementPosition);
                //checks if player is allowed to move to that side of the arena
                if (((filter.PlayerData->TeamNumber == BattleTeamNumber.TeamAlpha) && filter.PlayerData->TargetPosition.Y > 0)
                    || ((filter.PlayerData->TeamNumber == BattleTeamNumber.TeamBeta) && filter.PlayerData->TargetPosition.X < 0))
                {
                    filter.PlayerData->TargetPosition.Y = 0;
                }
                    Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", filter.PlayerData->TargetPosition);
            }

            if (input->RotateMotion)
            {
                FP maxAngle = FP.Rad_45;

                //stops player before rotation
                filter.PlayerData->TargetPosition = filter.Transform->Position;

                //rotates to right
                if (input->RotationDirection > 0 && filter.PlayerData->MovementRotation < maxAngle)
                {
                    filter.PlayerData->MovementRotation += rotationSpeed;
                    Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", filter.PlayerData->MovementRotation);
                }

                //rotates to left
                else if (input->RotationDirection < 0 && filter.PlayerData->MovementRotation > -maxAngle)
                {
                    filter.PlayerData->MovementRotation -= rotationSpeed;
                    Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", filter.PlayerData->MovementRotation);
                }
            }

            //returns player to 0 rotation when RotateMotion-input ends
            if (!input->RotateMotion && filter.PlayerData->MovementRotation != 0)
            {
                if (filter.PlayerData->MovementRotation > 0)
                    filter.PlayerData->MovementRotation -= rotationSpeed;

                else
                    filter.PlayerData->MovementRotation += rotationSpeed;
            }

            filter.Transform->Rotation = filter.PlayerData->BaseRotation + filter.PlayerData->MovementRotation;

            //moves player
            if (filter.Transform->Position != filter.PlayerData->TargetPosition)
                filter.Transform->Position = FPVector2.MoveTowards(filter.Transform->Position, filter.PlayerData->TargetPosition, filter.PlayerData->Speed * f.DeltaTime);

        }
    }
}
