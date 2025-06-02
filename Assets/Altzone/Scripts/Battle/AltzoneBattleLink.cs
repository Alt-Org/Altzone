
using Quantum;

using Battle.QSimulation.Game;

public static class AltzoneBattleLink
{
    public static void Int()
    {
        BattleAltzoneLink.InitLink(
            getCharacterPrototypeFnRef: GetCharacterPrototype
        );
    }

    private static AssetRef<EntityPrototype> GetCharacterPrototype(int characterID)
    {
        // add implementation
        return null;
    }
}
