using System;
using System.Diagnostics.CodeAnalysis;
using Prg.Scripts.Common.Extensions;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    public enum FurnitureSize
    {
        OneXOne,
        OneXTwo,
        OneXThree,
        OneXFour,
        OneXSix,
        TwoXOne,
        TwoXTwo,
        TwoXThree,
        TwoXFour,
        TwoXFive,
        TwoXSix,
        TwoXSeven,
        TwoXEight,
        ThreeXOne,
        ThreeXTwo,
        ThreeXThree,
        ThreeXFour,
        ThreeXFive,
        ThreeXSix,
        ThreeXSeven,
        ThreeXEight,
        FourXOne,
        FourXTwo,
        FourXThree,
        FourXFour,
        FiveXTwo,
        FiveXThree,
        FiveXFive,
        FiveXSix,
        FiveXEight,
        SixXFive,
        SevenXThree
    }

    public enum FurniturePlacement
    {
        Floor,
        FloorByWall,
        FloorNonblock,
        Ceiling,
        Wall

    }

    public enum FurnitureRarity
    {
        Common,
        Rare,
        Epic,
        Antique
    }

    /// <summary>
    /// Non-mutable <c>GameFurniture</c> for the game.
    /// </summary>
    /// <remarks>
    /// Fields in original source are:<br />
    /// ID	huonekalun nimi	muoto	paino / kg	materiaali	kierrätys	prefabin nimi	tiedoston nimi	kuva
    /// </remarks>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GameFurniture
    {
        public string Id;
        public string Name;
        public string Shape;
        public FurnitureRarity Rarity;
        public FurnitureSize Size;
        public FurnitureSize RotatedSize;
        public FurniturePlacement Placement;
        public double Weight;
        public float Value;
        public FurnitureInfo FurnitureInfo;
        public string Material;
        public string Recycling;

        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime. 
        /// </summary>
        public string UnityKey;

        public string Filename;

        public GameFurniture(string id, string name, FurnitureRarity rarity, FurnitureSize size, FurnitureSize rotatedSize, FurniturePlacement placementType, double weight, float value, string shape = "", string material = "", string recycling = "", string unityKey = "", string filename = "")
        {
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(name.IsSet());
            //Assert.IsTrue(shape.IsSet());
            Assert.IsTrue(weight >= 0);
            //Assert.IsTrue(material.IsSet());
            //Assert.IsTrue(recycling.IsSet());
            Assert.IsTrue(unityKey.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(filename.IsNullOEmptyOrNonWhiteSpace());
            Id = id;
            Name = name;
            Shape = shape;
            Rarity = rarity;
            Size = size;
            RotatedSize = rotatedSize;
            Placement = placementType;
            Weight = weight;
            Value = value;
            Material = material;
            Recycling = recycling;
            UnityKey = unityKey ?? string.Empty;
            Filename = filename ?? string.Empty;
        }

        public GameFurniture(string id, BaseFurniture baseFurniture, FurnitureInfo furnitureInfo, string unityKey = "", string filename = "")
        {
            Assert.IsTrue(id.IsSet());
            Assert.IsTrue(baseFurniture.Name.IsSet());
            //Assert.IsTrue(shape.IsMandatory());
            Assert.IsTrue(baseFurniture.Weight >= 0);
            //Assert.IsTrue(material.IsMandatory());
            //Assert.IsTrue(recycling.IsMandatory());
            Assert.IsTrue(unityKey.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(filename.IsNullOEmptyOrNonWhiteSpace());
            Id = id;
            Name = baseFurniture.Name;
            Shape = string.Empty;
            Rarity = baseFurniture.Rarity;
            Size = baseFurniture.Size;
            RotatedSize = baseFurniture.RotatedSize;
            Placement = baseFurniture.Placement;
            Weight = baseFurniture.Weight;
            Value = baseFurniture.Value;
            Material = baseFurniture.Material;
            Recycling = baseFurniture.Recycling;
            FurnitureInfo = furnitureInfo;
            UnityKey = unityKey ?? string.Empty;
            Filename = filename ?? string.Empty;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Shape)}: {Shape}, {nameof(Weight)}: {Weight}" +
                   $", {nameof(Material)}: {Material}, {nameof(Recycling)}: {Recycling}" +
                   $", {nameof(UnityKey)}: {UnityKey}, {nameof(Filename)}: {Filename}";
        }
    }

    [Serializable]
    public class BaseFurniture
    {
        public string Name;
        public FurnitureRarity Rarity;
        public FurnitureSize Size;
        public FurnitureSize RotatedSize;
        public FurniturePlacement Placement;
        public double Weight;
        public float Value;
        public string Material;
        public string Recycling;

        public override string ToString()
        {
            return $" {nameof(Name)}: {Name}, {nameof(Weight)}: {Weight}" +
                   $", {nameof(Material)}: {Material}, {nameof(Recycling)}: {Recycling}";
        }
    }
}
