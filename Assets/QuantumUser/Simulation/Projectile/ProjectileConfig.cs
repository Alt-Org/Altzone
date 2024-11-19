using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ProjectileConfig : AssetObject
    {
        [Tooltip("The speed that the projectile moves by default")]
        public FP ProjectileSpeed = 10;

        //TODO additional default references?
    }
}
