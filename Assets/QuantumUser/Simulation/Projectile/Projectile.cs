using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.QuantumUser.Simulation.Projectile
{
    [Preserve]
    public unsafe class Projectile : SystemMainThreadFilter<Projectile.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PhysicsBody2D* Body;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Update(Frame f, ref Filter filter) {
            filter.Body->AddForce(filter.Transform->Up);
        }
    }
}
