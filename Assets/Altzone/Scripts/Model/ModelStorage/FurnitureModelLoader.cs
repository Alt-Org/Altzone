using System.Collections.Generic;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts.Model.ModelStorage
{
    /// <summary>
    /// Utility class to load <c>FurnitureModel</c> models for runtime from external storage.
    /// </summary>
    internal static class FurnitureModelLoader
    {
        public static List<FurnitureModel> LoadModels()
        {
            return new List<FurnitureModel>
            {
                // These are for testing and can used by test scripts!
                new(1, FurnitureType.OneSquare, "Ruutu", "WhiteBall"),
                new(2, FurnitureType.TwoSquares, "Vaaka-2a", "CapsuleHorizontal"),
                new(3, FurnitureType.TwoSquares, "Vaaka-2b", "CapsuleVertical"),
                new(4, FurnitureType.ThreeSquaresBend, "Kolme palaa", "BendThreeSquares"),
                new(5, FurnitureType.OneSquare, "Test-5", "WhiteBall"),
                new(6, FurnitureType.Bomb, "Pommi", "Bomb"),
                new(7, FurnitureType.OneSquare, "Valkoinen pallo", "WhiteBall"),
                new(8, FurnitureType.OneSquare, "Ei löydy testi", "NotFoundPrefab"),
                new(9, FurnitureType.OneSquare, "Kirjain A", "LetterA"),
                new(10, FurnitureType.OneSquare, "Kirjain B", "LetterB"),
            };
        }
    }
}