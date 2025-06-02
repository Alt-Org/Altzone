/// @file BattleSoulWallQSpec.cs
/// <summary>
/// Asset that defines a list of usable SoulWall EntityPrototypes, which are used when creating SoulWalls.
/// </summary>
/// @bigtext{Filled with data from @ref BattleSoulWallQSpec.asset "BattleSoulWallQSpec" data asset.}

using Quantum;

namespace Battle.QSimulation.SoulWall
{
    /// <summary>
    /// Asset that defines a list of usable SoulWall EntityPrototypes
    /// </summary>
    ///
    /// @bigtext{Filled with data from @ref BattleSoulWallQSpec.asset "BattleSoulWallQSpec" data asset.}
    public class BattleSoulWallQSpec : AssetObject
    {
        /// <value>List of SoulWall EntityPrototypes</value>
        public AssetRef<EntityPrototype>[] SoulWallPrototypes;
    }
}
