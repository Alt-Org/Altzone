using System;

namespace Altzone.Scripts.Model
{
    [Serializable]
    public class InventoryItem
    {
        public int _id;
        public string _name;
        public int _furnitureId;

        public InventoryItem(int id, string name, int furnitureId)
        {
            _id = id;
            _name = name;
            _furnitureId = furnitureId;
        }

        public override string ToString()
        {
            return $"{nameof(_id)}: {_id}, {nameof(_name)}: {_name}, {nameof(_furnitureId)}: {_furnitureId}";
        }
    }
}