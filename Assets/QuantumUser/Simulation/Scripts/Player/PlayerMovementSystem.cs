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
            if (filter.PlayerData->Player == PlayerRef.None) return;
            Input* input = f.GetPlayerInput(filter.PlayerData->Player);

            UpdatePlayerMovement(f, ref filter, input);
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
