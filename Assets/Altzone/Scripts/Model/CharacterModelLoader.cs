using System.Collections.Generic;
using Altzone.Scripts.Config;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Utility class to load <c>CharacterModel</c> models for runtime from external storage.
    /// </summary>
    /// <remarks>
    /// WIKI: https://github.com/Alt-Org/Altzone/wiki/ModelLoader
    /// </remarks>
    public static class CharacterModelLoader
    {
        public static List<CharacterModel> LoadModels()
        {
            return new List<CharacterModel>()
            {
                new((int)Defence.Desensitisation, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 7, 3),
                new((int)Defence.Deflection, "Vitsiniekka", Defence.Deflection, 9, 3, 3, 4),
                new((int)Defence.Introjection, "Pappi", Defence.Introjection, 5, 5, 4, 4),
                new((int)Defence.Projection, "Taiteilija", Defence.Projection, 4, 2, 9, 5),
                new((int)Defence.Retroflection, "Hodariläski", Defence.Retroflection, 3, 7, 2, 9),
                new((int)Defence.Egotism, "Älykkö", Defence.Egotism, 6, 2, 6, 5),
                new((int)Defence.Confluence, "Tytöt", Defence.Confluence, 5, 6, 2, 6)
            };
        }
    }
}