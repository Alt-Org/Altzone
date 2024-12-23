using Photon.Deterministic;
using UnityEngine;

namespace Quantum.SoulWall
{
    public class SoulWallGameConfig: AssetObject
    {
        [Header("Soul Wall configuration")]
        [Tooltip("Collider configurations for soul walls")]
        public AssetRef<EntityPrototype>[] SoulWalls;
        [Tooltip("Initial health of the soul wall")]
        public FP InitialSoulWallHealth = 1;
    }
}
