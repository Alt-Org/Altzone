using System.Numerics;
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
            //public CharacterController2D* CharaCtrl;
            public PlayerLink* Link;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // gets the input for player 1
            Input* input = f.GetPlayerInput(0);

            UpdatePlayerMovement(f, ref filter, input);
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            FPVector3 targetPos;
            FPVector2 targetPos2D;

            // move somewhere else
            FP speed = 5;

            if (input->MouseClick)
            {
                targetPos = input->MousePosition;
                targetPos2D.X = targetPos.X;
                targetPos2D.Y = targetPos.Z;
                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", targetPos2D);
                filter.Transform->Position = FPVector2.MoveTowards(filter.Transform->Position, targetPos2D, speed * f.DeltaTime);
            }
        }
    }
}
