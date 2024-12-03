using Quantum;
using UnityEngine;
using Unity;

namespace Quantum
{


    public class ProjectileViewSFX : QuantumEntityViewComponent
    {
        public AudioSource wallAudioSource;
        public AudioSource soulWallAudioSource;
        public AudioSource shieldAudioSource;
        public AudioSource LaunchAudioSource;

        public override void OnInitialize()
        {

           // QuantumEvent.Subscribe<>(this, OnJumped);
           // QuantumEvent.Subscribe<EventJumpOnHook>(this, OnJumpOnHook);
           // QuantumEvent.Subscribe<EventAttacked>(this, OnAttacked);
        }


    }
}
