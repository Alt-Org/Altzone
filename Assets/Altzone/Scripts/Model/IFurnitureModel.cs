namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Allowed <c>FurnitureType</c> types.<br />
    /// Actually better name would be <c>FurnitureShape</c> (except for the Bomb).
    /// </summary>
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
    /// Furniture model for Clan Warehouse with default orientation (rotation up).
    /// </summary>
    /// <remarks>
    /// Furniture piece instances can be rotated when they are added into Clan Warehouse.
    /// </remarks>
    public interface IFurnitureModel
    {
        FurnitureType FurnitureType { get; }
        string Name { get; }
        string PrefabName { get; }
        int Id { get; }
    }
}