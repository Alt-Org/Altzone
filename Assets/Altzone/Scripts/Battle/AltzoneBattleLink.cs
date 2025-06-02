
using Quantum;

using Battle.QSimulation.Game;

using Altzone.Scripts.ModelV2;

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
        PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterID.ToString());
        return info != null ? info.BattleEntityPrototype : null;
    }
}
