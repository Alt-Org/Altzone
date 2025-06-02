/// @file BattleDiamondQSpec.cs
/// <summary>
/// Defines spawnable diamond's specifications.
/// </summary>
/// 
/// This spec defines spawnable diamond's Entity Prototype and the amount of diamonds spawned.<br/>  
/// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}

using Quantum;

namespace Battle.QSimulation.Diamond
{
    /// <summary>
    /// Asset that defines diamond settings.
    /// </summary>
    ///
    /// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}
    public class BattleDiamondQSpec : AssetObject
    {
        /// <value>Diamond Entity Prototype.</value>
        public AssetRef<EntityPrototype> DiamondPrototype;
        /// <value>Amount of diamonds spawned per destroyed SoulWall segment.</value>
        public int SpawnAmount;
    }
}
