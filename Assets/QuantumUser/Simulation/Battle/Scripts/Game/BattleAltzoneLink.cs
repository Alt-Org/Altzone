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
        public static AssetRef<EntityPrototype> GetCharacterPrototype(int id) => s_getCharacterPrototypeFnRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssetRef<EntityPrototype>[] GetShieldPrototypes(int id) => s_getShieldPrototypesFnRef(id);

        private static Func<int, AssetRef<EntityPrototype>> s_getCharacterPrototypeFnRef;
        private static Func<int, AssetRef<EntityPrototype>[]> s_getShieldPrototypesFnRef;
    }
}
