using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Game
{
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
        public int Id;
        public string Name;
        public string Shape;
        public double Weight;
        public string Material;
        public string Recycling;
        public string PrefabKey;
        public string Filename;

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Shape)}: {Shape}, {nameof(Weight)}: {Weight}" +
                   $", {nameof(Material)}: {Material}, {nameof(Recycling)}: {Recycling}" +
                   $", {nameof(PrefabKey)}: {PrefabKey}, {nameof(Filename)}: {Filename}";
        }
    }
}