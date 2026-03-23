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
            Func<int, AssetRef<EntityPrototype>> getCharacterPrototypeFnRef, Func<int, AssetRef<EntityPrototype>[]> getShieldPrototypesFnRef
        )
        {
            s_getCharacterPrototypeFnRef = getCharacterPrototypeFnRef;
            s_getShieldPrototypesFnRef = getShieldPrototypesFnRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetRef<EntityPrototype> GetCharacterPrototype(BattlePlayerCharacterID id) => s_getCharacterPrototypeFnRef((int)id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetRef<EntityPrototype>[] GetShieldPrototypes(BattlePlayerCharacterID id) => s_getShieldPrototypesFnRef((int)id);

        private static Func<int, AssetRef<EntityPrototype>> s_getCharacterPrototypeFnRef;
        private static Func<int, AssetRef<EntityPrototype>[]> s_getShieldPrototypesFnRef;
    }
}
