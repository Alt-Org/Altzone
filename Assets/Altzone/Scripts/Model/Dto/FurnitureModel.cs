using System.IO;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Data Transfer Object for <c>IFurnitureModel</c>.
    /// </summary>
    public class FurnitureModel : AbstractModel, IFurnitureModel
    {
        public FurnitureType FurnitureType { get; }
        public string Name { get; }
        public Color Color { get; }
        public string PrefabName { get; }

        public FurnitureModel(int id, FurnitureType furnitureType, string name, Color color, string prefabName) : base(id)
        {
            Assert.IsFalse(furnitureType == FurnitureType.Invalid);
            FurnitureType = furnitureType;
            Name = name;
            Color = color;
            PrefabName = prefabName;
        }

        public static GameObject Instantiate(IFurnitureModel model, Transform parent = null)
        {
            return Instantiate(model, Vector3.zero, Quaternion.identity, parent);
        }

        public static GameObject Instantiate(IFurnitureModel model, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var gameConfig = GameConfig.Get();
            var constants = gameConfig.Constants;
            var fullName = Path.Combine(constants._furniturePrefabFolder, model.PrefabName).Replace('\\', '/');
            var prefab = Resources.Load(fullName);
            if (prefab == null)
            {
                Debug.LogWarning($"prefab '{fullName}' was not found");
                return null;
            }
            var instance = (GameObject)Object.Instantiate(prefab, position, rotation, parent);
            instance.name = model.Name;
            return instance;
        }
    }
}