using System.IO;
using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

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
    /// Furniture model (for Raid mini-game and UI).
    /// </summary>
    public class FurnitureModel : AbstractModel
    {
        public readonly FurnitureType FurnitureType;
        public readonly string Name;
        public readonly Color Color;
        public readonly string PrefabName;

        public FurnitureModel(int id, FurnitureType furnitureType, string name, Color color, string prefabName) : base(id)
        {
            Assert.IsFalse(furnitureType == FurnitureType.Invalid);
            FurnitureType = furnitureType;
            Name = name;
            Color = color;
            PrefabName = prefabName;
        }

        public GameObject Instantiate(Transform parent)
        {
            var gameConfig = GameConfig.Get();
            var constants = gameConfig.Constants;
            var fullName = Path.Combine(constants._furniturePrefabFolder, PrefabName).Replace('\\', '/');
            var prefab = Resources.Load(fullName);
            if (prefab == null)
            {
                Debug.LogWarning($"prefab '{fullName}' was not found");
                return null;
            }
            var instance = (GameObject)Object.Instantiate(prefab, parent);
            instance.name = Name;
            return instance;
        }
    }
}