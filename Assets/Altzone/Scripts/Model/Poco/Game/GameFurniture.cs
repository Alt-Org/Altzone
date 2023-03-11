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
        private string Name;
        private string Shape;
        private int Weight;
        private string Material;
        private string Recycling;
        public string PrefabKey;
        private string Filename;
    }
}