using System.Collections.Generic;

namespace Altzone.Scripts.Model.Loader
{
    /// <summary>
    /// Utility class to load <c>CharacterClassModel</c> models for runtime from external storage.
    /// </summary>
    public static class CharacterModelLoader
    {
        public static List<CharacterClassModel> LoadModels()
        {
            return new List<CharacterClassModel>()
            {
                new(1, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 7, 3),
                new(2, "Vitsiniekka", Defence.Deflection, 9, 3, 3, 4),
                new(3, "Pappi", Defence.Introjection, 5, 5, 4, 4),
                new(4, "Taiteilija", Defence.Projection, 4, 2, 9, 5),
                new(5, "Hodariläski", Defence.Retroflection, 3, 7, 2, 9),
                new(6, "Älykkö", Defence.Egotism, 6, 2, 6, 5),
                new(7, "Tytöt", Defence.Confluence, 5, 6, 2, 6)
            };
        }
    }
}