using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using Photon.Deterministic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerClassDesensitizerProjectileQSystem : SystemMainThreadFilter<BattlePlayerClassDesensitizerProjectileQSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public BattlePlayerClassDesensitizerProjectileQComponent* Projectile;
        }

        
        public static void Create(Frame f, EntityPrototype entityPrototype, FPVector2 position, FPVector2 direction, FP speed)
        {
            // f.Create // Create a prjoctile based on prototype.
            EntityRef entityRef = f.Create(entityPrototype);
            // f.unsafe.GetPointer() // get BattlePlayerClassDesensitizerProjectileQComponent Projectile;
            Transform2D* transform = f.Unsafe.GetPointer<Transform2D>(entityRef);
            BattlePlayerClassDesensitizerProjectileQComponent* projectile = f.Unsafe.GetPointer<BattlePlayerClassDesensitizerProjectileQComponent>(entityRef);
            // Set Initial projectile direction and speed
            transform->Position = position;
            projectile->Direction = direction;
            projectile->Speed = speed;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.Transform->Position += filter.Projectile->Direction * filter.Projectile->Speed;
        }

        

        /*
        When projectile hits the emotion object
        -> emotion object changes direction to this projectile's direction
        - Which script control's emotion object's movement direction?
        */
    }
}
