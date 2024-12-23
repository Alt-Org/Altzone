using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ProjectileConfig : AssetObject
    {
        [Tooltip("The speed that the projectile moves by default")]
        public FP ProjectileSpeed = 10;
        public FP Cooldown; // Add a cooldown field
        //TODO additional default references?
    }
}
