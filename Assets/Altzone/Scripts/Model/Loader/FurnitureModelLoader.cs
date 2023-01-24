using System.Collections.Generic;
using Altzone.Scripts.Model.Dto;
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
            return new List<FurnitureModel>
            {
                // These are for testing and can used by test scripts!
                new(1, FurnitureType.OneSquare, "OneSquare", Color.red, "WhiteBall"),
                new(2, FurnitureType.TwoSquares, "TwoSquares", Color.red, "WhiteBall"),
                new(3, FurnitureType.ThreeSquaresStraight, "ThreeSquaresStraight", Color.red, "WhiteBall"),
                new(4, FurnitureType.ThreeSquaresBend, "ThreeSquaresBend", Color.red, "WhiteBall"),
                new(5, FurnitureType.FourSquares, "FourSquares", Color.red, "WhiteBall"),
                new(6, FurnitureType.Bomb, "Bomb", Color.black, "Bomb"),
                new(7, FurnitureType.OneSquare, "WhiteBall", Color.white, "WhiteBall"),
                new(8, FurnitureType.OneSquare, "NotFoundPrefab", Color.red, "NotFoundPrefab"),
                new(9, FurnitureType.OneSquare, "LetterA", Color.red, "LetterA"),
                new(10, FurnitureType.OneSquare, "LetterB", Color.red, "LetterB"),
            };
        }
    }
}