/// @file BattlePlayerHitboxQComponent.cs
/// <summary>
/// Contains @cref{Quantum,BattlePlayerHitboxQComponent} partial struct, which adds a method for calculating normals to the Quantum Component.
/// </summary>

//Quantum usings
using Photon.Deterministic;

namespace Quantum
{
    // Adds functionality to calculate normals.
    // Main struct definition in BattlePlayerHitbox.qtn.
    // Main struct documentation in qtn-BattlePlayerHitboxQComponent.dox.
    public unsafe partial struct BattlePlayerHitboxQComponent
    {
        /// <summary>
        /// Calculates the normal of this hitbox.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>calculated normal.</returns>
        public readonly FPVector2 CalculateNormal(Frame f)
        {
            Transform2D* parentTransform = f.Unsafe.GetPointer<Transform2D>(ParentEntityRef);

            FP normalAngleRad = parentTransform->Rotation + NormalAngleRad;
            return FPVector2.Rotate(FPVector2.Up, normalAngleRad);
        }
    }
}
