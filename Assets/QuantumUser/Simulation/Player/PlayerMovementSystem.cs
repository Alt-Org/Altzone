using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        private FPVector2 _targetPos2D;
        private bool _startMovement = false;

        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerData* PlayerData;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            Input* input = default;
            if(f.Unsafe.TryGetPointer(filter.Entity, out PlayerData* playerData))
            {
                input = f.GetPlayerInput(playerData->Player);
            }

            UpdatePlayerMovement(f, ref filter, input);
        }

        private void UpdatePlayerMovement(Frame f, ref Filter filter, Input* input)
        {
            FPVector3 targetPos;

            if (input->MouseClick)
            {
                targetPos = input->MousePosition;
                _targetPos2D.X = targetPos.X;
                _targetPos2D.Y = targetPos.Z;
                Debug.LogFormat("[PlayerMovementSystem] Mouse clicked (mouse position: {0}", _targetPos2D);
                _startMovement = true;
            }

            if(filter.Transform->Position != _targetPos2D && _startMovement)
            {
                filter.Transform->Position = FPVector2.MoveTowards(filter.Transform->Position, _targetPos2D, filter.PlayerData->Speed * f.DeltaTime);
            }
        }
    }
}
