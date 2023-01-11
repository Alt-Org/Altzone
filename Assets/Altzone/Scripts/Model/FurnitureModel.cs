using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    public enum FurnitureType
    {
        Invalid = 0,
        OneSquare = 1,
        TwoSquares = 2,
        ThreeSquaresStraight = 3,
        ThreeSquaresBend = 4,
        FourSquares = 5,
    }

    /// <summary>
    /// Furniture model (for Raid mini-game and UI).
    /// </summary>
    public class FurnitureModel : AbstractModel
    {
        public readonly FurnitureType FurnitureType;
        public readonly string Name;
        public readonly Color Color;
        public readonly string PrefabName;

        public FurnitureModel(int id, FurnitureType furnitureType, string name, Color color, string prefabName) : base(id)
        {
            Assert.IsFalse(furnitureType == FurnitureType.Invalid);
            FurnitureType = furnitureType;
            Name = name;
            Color = color;
            PrefabName = prefabName;
        }
    }
}