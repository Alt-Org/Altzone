using UnityEngine;

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
        Bomb = 6,
    }

    /// <summary>
    /// Furniture model for Clan Warehouse.
    /// </summary>
    public interface IFurnitureModel
    {
        FurnitureType FurnitureType { get; }
        string Name { get; }
        Color Color { get; }
        string PrefabName { get; }
        int Id { get; }
    }
}