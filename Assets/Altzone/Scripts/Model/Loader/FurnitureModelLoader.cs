using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Loader
{
    /// <summary>
    /// Utility class to load <c>FurnitureModel</c> models for runtime from external storage.
    /// </summary>
    public static class FurnitureModelLoader
    {
        public static List<FurnitureModel> LoadModels()
        {
            return new List<FurnitureModel>()
            {
                new(1, FurnitureType.OneSquare, "OneSquare", Color.red, "OneSquare"),
                new(2, FurnitureType.TwoSquares, "TwoSquares", Color.red, "TwoSquares"),
                new(3, FurnitureType.ThreeSquaresStraight, "ThreeSquaresStraight", Color.red, "ThreeSquaresStraight"),
                new(4, FurnitureType.ThreeSquaresBend, "ThreeSquaresBend", Color.red, "ThreeSquaresBend"),
                new(5, FurnitureType.FourSquares, "FourSquares", Color.red, "FourSquares"),
                new(6, FurnitureType.OneSquare, "WhiteBall", Color.white, "WhiteBall"),
            };
        }
    }
}