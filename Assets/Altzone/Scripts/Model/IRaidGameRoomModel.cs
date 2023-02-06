using System.Collections.Generic;

namespace Altzone.Scripts.Model
{
    public interface IRaidRoomLocation
    {
        int X { get; }
        int Y { get; }
    }

    public interface IRaidCoinLocation : IRaidRoomLocation
    {
        int Amount { get; }
    }
    
    public interface IRaidFurnitureLocation : IRaidRoomLocation
    {
        /// <summary>
        /// Furniture Model Id in game furniture models storage.
        /// </summary>
        int FurnitureId { get; }

        /// <summary>
        /// Furniture rotation 0, 90, 180 or 270.
        /// </summary>
        int Rotation { get; }

        /// <summary>
        /// Furniture sorting order on layer.
        /// </summary>
        int SortingOrder { get; }

    }

    /// <summary>
    /// Game room model for Clan Warehouse and Raid mini-game.
    /// </summary>
    public interface IRaidGameRoomModel
    {
        int Id { get; }
        string Name { get; }
        int Width { get; }
        int Height { get; }
        int MatchMakingValue { get; }

        List<IRaidRoomLocation> FreeSpaceLocations { get; }
        List<IRaidCoinLocation> CoinLocations { get; }
        List<IRaidFurnitureLocation> FurnitureLocations { get; }
    }
}