using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.Common
{
    public enum Rarity
    {
        None,
        Common,
        Rare,
        Epic,
        Antique
    }
}
namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/RarityColourReference", fileName = "RarityColourReference")]
    public class RarityColourReference : ScriptableObject
    {
        [Header("Rarity Color")]
        [SerializeField] private Color _commonColor;
        [SerializeField] private Color _rareColor;
        [SerializeField] private Color _epicColor;
        [SerializeField] private Color _antiqueColor;

        public Color CommonColor { get => _commonColor; }
        public Color RareColor { get => _rareColor; }
        public Color EpicColor { get => _epicColor; }
        public Color AntiqueColor { get => _antiqueColor; }

        public Color GetColor(Rarity classId)
        {
            return classId switch
            {
                Rarity.Common => _commonColor,
                Rarity.Rare => _rareColor,
                Rarity.Epic => _epicColor,
                Rarity.Antique => _antiqueColor,
                _ => Color.white,
            };
        }

        public Color GetColor(FurnitureRarity rarity)
        {
            return rarity switch
            {
                FurnitureRarity.Common => GetColor(Rarity.Common),
                FurnitureRarity.Rare => GetColor(Rarity.Rare),
                FurnitureRarity.Epic => GetColor(Rarity.Epic),
                FurnitureRarity.Antique => GetColor(Rarity.Antique),
                _ => GetColor(Rarity.None),
            };
        }
    }
}
