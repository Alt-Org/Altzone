namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Inventory item (now only for furniture).
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>
        /// Furniture id for inventory.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Furniture name (override) or localization key for UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Furniture Model Id in game furniture models storage.
        /// </summary>
        int FurnitureId { get; }
    }
}