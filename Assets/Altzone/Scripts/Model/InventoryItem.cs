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

        public InventoryItem(int id, string name, InventoryItemType type)
        {
            _id = id;
            _name = name;
            _type = type;
        }

        public override string ToString()
        {
            return $"{nameof(_id)}: {_id}, {nameof(_name)}: {_name}, {nameof(_type)}: {_type}";
        }
    }
}