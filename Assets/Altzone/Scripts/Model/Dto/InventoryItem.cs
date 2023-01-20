using System;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IFurnitureModel</c>.
    /// </summary>
    [Serializable]
    public class InventoryItem : IInventoryItem
    {
        public int _id;
        public string _name;
        public int _furnitureId;

        public int Id => _id;

        public string Name => _name;

        public int FurnitureId => _furnitureId;

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