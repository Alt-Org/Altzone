
using System.Runtime.CompilerServices;
using Quantum;

using Altzone.Scripts.ModelV2;
using Battle.QSimulation.Game;

public static class AltzoneBattleLink
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init() => BattleAltzoneLink.InitLink(
        getCharacterPrototypeFnRef: GetCharacterPrototype
    );

    private static AssetRef<EntityPrototype> GetCharacterPrototype(int characterID)
    {
        PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterID.ToString());
        return info != null ? info.BattleEntityPrototype : null;
    }
}
