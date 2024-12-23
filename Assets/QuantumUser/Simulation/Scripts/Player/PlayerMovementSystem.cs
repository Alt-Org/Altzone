using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

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
            Input* input = default;
            if (f.Unsafe.TryGetPointer(filter.Entity, out PlayerData* playerData))
            {
                input = f.GetPlayerInput(playerData->Player);
            }
            UpdatePlayerMovement(f, ref filter, input);
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            FP rotationSpeed = FP._0_20;

            if (input->MouseClick)
            {
                filter.PlayerData->TargetPosition.X = input->MousePosition.X;
                filter.PlayerData->TargetPosition.Y = input->MousePosition.Z;
                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", filter.PlayerData->TargetPosition);
            }

            if (input->RotateMotion)
            {
                FP maxAngle = FP._1;

                //stops player before rotation
                filter.PlayerData->TargetPosition = filter.Transform->Position;

                //rotates to right
                if (input->RotationDirection > 0 && filter.Transform->Rotation < maxAngle)
                {
                    filter.Transform->Rotation += rotationSpeed;
                    Debug.LogFormat("[PlayerRotatingSystem] Leaning right(rotation: {0}", filter.Transform->Rotation);
                }

                //rotates to left
                else if (input->RotationDirection < 0 && filter.Transform->Rotation > -maxAngle)
                {
                    filter.Transform->Rotation -= rotationSpeed;
                    Debug.LogFormat("[PlayerRotatingSystem] Leaning left(rotation: {0}", filter.Transform->Rotation);
                }

            }

            //returns player to 0 rotation when RotateMotion-input ends
            if (!input->RotateMotion && filter.Transform->Rotation != 0)
            {
                if (filter.Transform->Rotation > 0)
                    filter.Transform->Rotation -= rotationSpeed;

                else
                    filter.Transform->Rotation += rotationSpeed;
            }

            //moves player
            if (filter.Transform->Position != filter.PlayerData->TargetPosition)
                filter.Transform->Position = FPVector2.MoveTowards(filter.Transform->Position, filter.PlayerData->TargetPosition, filter.PlayerData->Speed * f.DeltaTime);

        }
    }
}
