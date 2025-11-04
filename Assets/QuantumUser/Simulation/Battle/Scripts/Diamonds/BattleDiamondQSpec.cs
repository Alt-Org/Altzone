/// @file BattleDiamondQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Diamond,BattleDiamondQSpec} class for defining spawnable diamond's specifications.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Diamond
{
    /// <summary>
    /// Class for defining spawnable diamond's specifications.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// Can be used to make multiple %BattleDiamondQSpec data assets.<br/>
    /// @bigtext{Filled with data from @ref BattleDiamondQSpec.asset "BattleDiamondQSpec" data asset.}
    public class BattleDiamondQSpec : AssetObject
    {
        /// <value>Diamond Entity Prototype.</value>
        public AssetRef<EntityPrototype> DiamondPrototype;
        /// <value>Amount of diamonds spawned per destroyed SoulWall segment.</value>
        public int SpawnAmount;
        /// <value>The angle range within which a diamond's launch angle is selected from.</value>
        public FP SpawnAngleDeg;
        /// <value>The minimum travel distance a diamond can be assigned.</value>
        public FP TravelDistanceMin;
        /// <value>The maximum travel distance a diamond can be assigned.</value>
        public FP TravelDistanceMax;
        /// <value>The travel speed of diamonds.</value>
        public FP TravelSpeed;
        /// <value>The vertical distance from a diamonds target distance at which the diamond will begin slowing down.</value>
        public FP BreakDistance;
        /// <value>The force by which a diamond slows down.</value>
        public FP BreakForce;
        /// <value>Time in seconds a diamond will remain on the arena after it stops traveling.</value>
        public FP LifetimeSec;
    }
}
