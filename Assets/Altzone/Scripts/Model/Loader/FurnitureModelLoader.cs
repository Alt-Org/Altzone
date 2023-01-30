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
                new(1, FurnitureType.OneSquare, "Ruutu", "WhiteBall"),
                new(2, FurnitureType.TwoSquares, "Tuplaruutu", "WhiteBall"),
                new(3, FurnitureType.ThreeSquaresStraight, "Kolmen suora", "WhiteBall"),
                new(4, FurnitureType.ThreeSquaresBend, "L-mutka", "WhiteBall"),
                new(5, FurnitureType.FourSquares, "Neliö", "WhiteBall"),
                new(6, FurnitureType.Bomb, "Pommi", "Bomb"),
                new(7, FurnitureType.OneSquare, "Valkoinen pallo", "WhiteBall"),
                new(8, FurnitureType.OneSquare, "Ei löydy testi", "NotFoundPrefab"),
                new(9, FurnitureType.OneSquare, "Kirjain A", "LetterA"),
                new(10, FurnitureType.OneSquare, "Kirjain B", "LetterB"),
            };
        }
    }
}