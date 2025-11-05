// System usings
using System;
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;

namespace Battle.QSimulation.Game
{
    public static class BattleAltzoneLink
    {
        public static void InitLink(
            Func<int, AssetRef<EntityPrototype>> getCharacterPrototypeFnRef
        )
        {
            s_getCharacterPrototypeFnRef = getCharacterPrototypeFnRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetRef<EntityPrototype> GetCharacterPrototype(int id) => s_getCharacterPrototypeFnRef(id);

        private static Func<int, AssetRef<EntityPrototype>> s_getCharacterPrototypeFnRef;
    }
}
