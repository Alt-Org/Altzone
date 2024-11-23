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
            // gets the input for player 1
            Input* input = f.GetPlayerInput(0);

            UpdatePlayerMovement(f, ref filter, input);
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
<<<<<<< Updated upstream
            FPVector3 targetPos;
            
=======
>>>>>>> Stashed changes
            if (input->MouseClick)
            {
                filter.PlayerData->TargetPos = input->MousePosition;
                filter.PlayerData->TargetPos2D.X = filter.PlayerData->TargetPos.X;
                filter.PlayerData->TargetPos2D.Y = filter.PlayerData->TargetPos.Z;
                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", filter.PlayerData->TargetPos2D);
                filter.PlayerData->isAllowedToMove = true;
            }

            if(filter.PlayerData->CurrentPos != filter.PlayerData->TargetPos2D && filter.PlayerData->isAllowedToMove)
            {
                filter.Transform->Position = FPVector2.MoveTowards(filter.PlayerData->CurrentPos, filter.PlayerData->TargetPos2D, filter.PlayerData->Speed * f.DeltaTime);
                filter.PlayerData->CurrentPos = filter.Transform->Position;
            }
        }
    }
}
