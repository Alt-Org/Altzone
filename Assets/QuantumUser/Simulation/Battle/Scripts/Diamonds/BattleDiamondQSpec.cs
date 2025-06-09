/// @file BattleDiamondQSpec.cs
/// <summary>
/// Class for defining spawnable diamond's specifications.
/// </summary>
/// 
/// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}

using Quantum;

namespace Battle.QSimulation.Diamond
{
    /// <summary>
    /// Class for defining spawnable diamond's specifications.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.  
    /// Can be used to make multiple %BattleDiamondQSpec data assets.<br/>  
    /// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}
    public class BattleDiamondQSpec : AssetObject
    {
        /// <value>Diamond Entity Prototype.</value>
        public AssetRef<EntityPrototype> DiamondPrototype;
        /// <value>Amount of diamonds spawned per destroyed SoulWall segment.</value>
        public int SpawnAmount;
    }
}
