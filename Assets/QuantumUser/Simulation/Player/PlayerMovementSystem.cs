namespace Quantum
{
    using System.Numerics;
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>{

        FPVector3 _targetPos;
        FPVector2 _targetPos2D;

        
        public override void Update(Frame f, ref Filter filter){
            // gets the input for player 1
            var input = f.GetPlayerInput(0);

            UpdatePlayerMovement(f, ref filter, input);
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input){
            
            if (input->MouseClick){
                _targetPos = input -> MousePosition;
                _targetPos2D.X = _targetPos.X;
                _targetPos2D.Y = _targetPos.Y;
                Debug.Log("Mouse clicked, mouseposition: " + _targetPos2D);
                filter.CharaCtrl -> Move(f, filter.Entity, _targetPos2D);
                
            }

            //filter.CharaCtrl -> Move(f, filter.Entity, _targetPos2D);

        }       


        public struct Filter{
            public EntityRef Entity;
            public Transform2D* Transform;
            public CharacterController2D* CharaCtrl;
            public PlayerLink* Link;

        }
    }
}
