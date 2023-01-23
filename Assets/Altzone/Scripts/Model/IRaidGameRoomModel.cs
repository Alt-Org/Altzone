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
        int FurnitureId { get; }
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