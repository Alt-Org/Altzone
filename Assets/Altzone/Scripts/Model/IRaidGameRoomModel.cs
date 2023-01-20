using System.Collections.Generic;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts.Model
{
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

        List<RaidGameRoomModel.FreeSpaceLocation> FreeSpaceLocations { get; }
        List<RaidGameRoomModel.CoinLocation> CoinLocations { get; }
        List<RaidGameRoomModel.FurnitureLocation> FurnitureLocations { get; }
    }
}