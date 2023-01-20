namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Inventory item (now only for furniture).
    /// </summary>
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int FurnitureId { get; }
    }
}