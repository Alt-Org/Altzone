//Quantum usings
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct BattlePlayerHitboxQComponent
    {
        public readonly FPVector2 CalculateNormal(Frame f)
        {
            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(ParentEntityRef);

            FP normalAngleRad = parentTransform->Rotation + NormalAngleRad;
            return FPVector2.Rotate(FPVector2.Up, normalAngleRad);
        }
    }
}
