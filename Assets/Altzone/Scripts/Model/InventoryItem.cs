using System;

namespace Altzone.Scripts.Model
{
    public enum InventoryItemType
    {
        Invalid = 0,
        Furniture = 1,
    }
    
    [Serializable]
    public class InventoryItem
    {
        public int _id;
        public string _name;
        public InventoryItemType _type;
    }
}