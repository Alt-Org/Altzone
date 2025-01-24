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
                //checks if player is allowed to move to that side of the arena
                if (((filter.PlayerData->Player == 0 || filter.PlayerData->Player == 1) && input->MousePosition.Z < 0)
                    || ((filter.PlayerData->Player == 2 || filter.PlayerData->Player == 3) && input->MousePosition.Z > 0))
                {
                    filter.PlayerData->TargetPosition.X = input->MousePosition.X;
                    filter.PlayerData->TargetPosition.Y = input->MousePosition.Z;
                    Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", filter.PlayerData->TargetPosition);
                }
                //if player is not allowed to move to that side of the arena, targetposition is for as far as a player can go
                else
                {
                    filter.PlayerData->TargetPosition.X = input->MousePosition.X;
                    filter.PlayerData->TargetPosition.Y = 0;
                }
            }

            if (input->RotateMotion)
            {
                FP maxAngleAlphaTeam = FP.Rad_45;
                FP maxAngleBetaTeamClockWise = FP.Rad_180 - FP.Rad_45;
                FP maxAngleBetaTeamCounterClockWise = FP.Rad_180 + FP.Rad_45;

                //stops player before rotation
                filter.PlayerData->TargetPosition = filter.Transform->Position;

                //rotation for AlphaTeam players
                if (filter.PlayerData->Player == 0 || filter.PlayerData->Player == 1)
                {
                    //rotates to right
                    if (input->RotationDirection > 0 && filter.Transform->Rotation < maxAngleAlphaTeam)
                    {
                        filter.Transform->Rotation += rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] AlphaTeam player leaning right(rotation: {0}", filter.Transform->Rotation);
                    }

                    //rotates to left
                    else if (input->RotationDirection < 0 && filter.Transform->Rotation > -maxAngleAlphaTeam)
                    {
                        filter.Transform->Rotation -= rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] AlphaTeam player leaning left(rotation: {0}", filter.Transform->Rotation);
                    }
                }

                //rotation for BetaTeam players
                if (filter.PlayerData->Player == 2 || filter.PlayerData->Player == 3)
                {
                    //rotates to right
                    if (input->RotationDirection > 0 && filter.Transform->Rotation < maxAngleBetaTeamCounterClockWise)
                    {
                        filter.Transform->Rotation += rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] BetaTeam player leaning right(rotation: {0}", filter.Transform->Rotation);
                    }

                    //rotates to left
                    else if (input->RotationDirection < 0 && filter.Transform->Rotation > maxAngleBetaTeamClockWise)
                    {
                        filter.Transform->Rotation -= rotationSpeed;
                        Debug.LogFormat("[PlayerRotatingSystem] BetaTeam player leaning left(rotation: {0}", filter.Transform->Rotation);
                    }
                }

            }

            //returns player to 0 rotation when RotateMotion-input ends
            if (!input->RotateMotion)
            {
                //for AlphaTeam Players that are rotated
                if ((filter.PlayerData->Player == 0 || filter.PlayerData->Player == 1) && filter.Transform->Rotation != 0)
                {
                    if (filter.Transform->Rotation > 0)
                        filter.Transform->Rotation -= rotationSpeed;

                    else
                        filter.Transform->Rotation += rotationSpeed;
                }

                //for BetaTeam Players that are rotated
                if ((filter.PlayerData->Player == 2 || filter.PlayerData->Player == 3) && filter.Transform->Rotation != FP.Rad_180)
                {
                    if (filter.Transform->Rotation > FP.Rad_180)
                        filter.Transform->Rotation -= rotationSpeed;

                    else
                        filter.Transform->Rotation += rotationSpeed;
                }

            }


            //moves player
            if (filter.Transform->Position != filter.PlayerData->TargetPosition)
                filter.Transform->Position = FPVector2.MoveTowards(filter.Transform->Position, filter.PlayerData->TargetPosition, filter.PlayerData->Speed * f.DeltaTime);

        }
    }
}
