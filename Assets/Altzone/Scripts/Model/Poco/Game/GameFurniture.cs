using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Non-mutable <c>GameFurniture</c> for the game.
    /// </summary>
    /// <remarks>
    /// Fields in original source are:<br />
    /// ID	huonekalun nimi	muoto	paino / kg	materiaali	kierrätys	prefabin nimi	tiedoston nimi	kuva
    /// </remarks>
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GameFurniture
    {
        [PrimaryKey] public string Id;
        [Unique] public string Name;
        [Mandatory] public string Shape;
        public double Weight;
        [Mandatory] public string Material;
        [Mandatory] public string Recycling;

        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime. 
        /// </summary>
        [Optional] public string UnityKey;

        public string Filename;

        public GameFurniture(string id, string name, string shape, double weight, string material, string recycling, string unityKey, string filename)
        {
            Assert.IsTrue(id.IsPrimaryKey());
            Assert.IsTrue(name.IsMandatory());
            Assert.IsTrue(shape.IsMandatory());
            Assert.IsTrue(weight >= 0);
            Assert.IsTrue(material.IsMandatory());
            Assert.IsTrue(recycling.IsMandatory());
            Assert.IsTrue(unityKey.IsNullOEmptyOrNonWhiteSpace());
            Assert.IsTrue(filename.IsNullOEmptyOrNonWhiteSpace());
            Id = id;
            Name = name;
            Shape = shape;
            Weight = weight;
            Material = material;
            Recycling = recycling;
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
}