using Quantum;

namespace Battle.QSimulation.SoulWall
{
    /// <summary>
    /// Asset that defines a list of usable SoulWall EntityPrototypes
    /// </summary>
    public class BattleSoulWallQSpec : AssetObject
    {
        /// <value>List of SoulWall EntityPrototypes</value>
        public AssetRef<EntityPrototype>[] SoulWallPrototypes;
    }
}
