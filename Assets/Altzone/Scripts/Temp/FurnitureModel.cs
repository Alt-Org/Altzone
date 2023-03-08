using System.IO;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Temp
{
    /// <summary>
    /// Data Transfer Object for <c>IFurnitureModel</c>.
    /// </summary>
    public class FurnitureModel : AbstractModel, IFurnitureModel
    {
        public FurnitureType FurnitureType { get; }
        public string Name { get; }
        public string PrefabName { get; }

        public FurnitureModel(int id, FurnitureType furnitureType, string name, string prefabName) : base(id)
        {
            Assert.IsFalse(furnitureType == FurnitureType.Invalid);
            FurnitureType = furnitureType;
            Name = name;
            PrefabName = prefabName;
        }

        public static GameObject Instantiate(IFurnitureModel model, Transform parent = null)
        {
            return Instantiate(model, Vector3.zero, Quaternion.identity, 0, parent);
        }

        public static GameObject Instantiate(IFurnitureModel model, Vector3 position, Quaternion rotation, int sortingOrder = 0, Transform parent = null)
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
            if (sortingOrder == 0)
            {
                return instance;
            }
            // Assume that we want set SpriteRenderer sortingOrder only if it is not the default (0)!
            Assert.IsTrue(sortingOrder is >= -32768 and <= 32767);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sortingOrder = sortingOrder;
            }
            return instance;
        }
    }
}