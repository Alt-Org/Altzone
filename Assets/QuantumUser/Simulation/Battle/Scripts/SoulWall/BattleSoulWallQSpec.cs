/// @file BattleSoulWallQSpec.cs
/// <summary>
/// Class for defining a list of usable SoulWall EntityPrototypes for creating SoulWalls.
/// </summary>
/// @bigtext{Filled with data from @ref BattleSoulWallQSpec.asset "BattleSoulWallQSpec" data asset.}

using Quantum;

namespace Battle.QSimulation.SoulWall
{
    /// <summary>
    /// Class for defining a list of usable SoulWall EntityPrototypes.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.  
    /// Can be used to make multiple %BattleSoulWallQSpec data assets.<br/>  
    /// @bigtext{Filled with data from @ref BattleSoulWallQSpec.asset "BattleSoulWallQSpec" data asset.}
    public class BattleSoulWallQSpec : AssetObject
    {
        /// <value>List of SoulWall EntityPrototypes</value>
        public AssetRef<EntityPrototype>[] SoulWallPrototypes;
    }
}
