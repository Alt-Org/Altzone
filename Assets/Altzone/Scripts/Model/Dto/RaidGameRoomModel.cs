using System;
using System.Collections.Generic;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Serializable Room model for Raid mini-game that can be sent over network or saved somewhere (e.h. as JSON string).
    /// </summary>
    /// <remarks>
    /// Data coordinate system is: X = colum, Y = row, origo = top,left, zero based indexing.
    /// </remarks>
    [Serializable]
    public class RaidGameRoomModel : IRaidGameRoomModel
    {
        [Serializable]
        public class FreeSpaceLocation
        {
            public int _x;
            public int _y;

            public FreeSpaceLocation(int x, int y)
            {
                _x = x;
                _y = y;
            }
        }

        [Serializable]
        public class CoinLocation
        {
            public int _x;
            public int _y;
            public int _amount;

            public CoinLocation(int x, int y, int amount)
            {
                _x = x;
                _y = y;
                _amount = amount;
            }
        }

        [Serializable]
        public class FurnitureLocation
        {
            public int _x;
            public int _y;
            public int _furnitureId;

            public FurnitureLocation(int x, int y, int furnitureId)
            {
                _x = x;
                _y = y;
                _furnitureId = furnitureId;
            }
        }

        public int _id;
        public string _name;
        public int _width;
        public int _height;
        public int _matchMakingValue;

        public List<FreeSpaceLocation> _freeSpaceLocations = new();
        public List<CoinLocation> _coinLocations = new();
        public List<FurnitureLocation> _furnitureLocations = new();

        public int Id => _id;

        public string Name => _name;

        public int Width => _width;

        public int Height => _height;

        public int MatchMakingValue => _matchMakingValue;

        public List<FreeSpaceLocation> FreeSpaceLocations => _freeSpaceLocations;

        public List<CoinLocation> CoinLocations => _coinLocations;

        public List<FurnitureLocation> FurnitureLocations => _furnitureLocations;

        public RaidGameRoomModel(int id, string name, int width, int height, int matchMakingValue = 0)
        {
            _id = id;
            _name = name;
            _width = width;
            _height = height;
            _matchMakingValue = matchMakingValue;
        }
    }
}