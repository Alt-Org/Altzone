using System;
using System.Collections.Generic;
using System.Linq;

namespace Altzone.Scripts.Model.Loader
{
    /// <summary>
    /// Helper class for external <c>CustomCharacterModel</c> operations.
    /// </summary>
    public static class CustomCharacterModels
    {
        public static CustomCharacterModel GetCustomCharacterModel(int id)
        {
            return LoadModels().FirstOrDefault(x => x.Id == id);
        }

        public static List<CustomCharacterModel> LoadModels()
        {
            return new List<CustomCharacterModel>()
            {
                new(1, 1, "Koulukiusaaja", 0, 0, 0, 0),
                new(2, 2, "Vitsiniekka", 0, 0, 0, 0),
                new(3, 2, "Pappi", 0, 0, 0, 0),
                new(4, 4, "Taiteilija", 0, 0, 0, 0),
                new(5, 5, "Hodariläski", 0, 0, 0, 0),
                new(6, 6, "Älykkö", 0, 0, 0, 0),
                new(7, 7, "Tytöt", 0, 0, 0, 0),
            };
        }

        public static void Save(CustomCharacterModel customCharacterModel)
        {
            throw new NotImplementedException();
        }
    }
}