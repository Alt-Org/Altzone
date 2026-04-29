using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public struct BattleCharacterBase
    {
        public int Id;
        public int Class;

        public BattlePlayerStats Stats;

        /// <summary>
        /// Pre-resolved entity prototype reference for this character.
        /// Set in View code (SetPlayerQuantumCharacters) so that Simulation code
        /// never needs to call into View-layer delegates, which would break determinism.
        /// </summary>
        public AssetRef<EntityPrototype> Prototype;
    }
}
