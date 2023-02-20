using System.Collections.Generic;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts.Model.ModelStorage
{
    /// <summary>
    /// Utility class to load <c>ClanModel</c> models for runtime from external storage.
    /// </summary>
    internal static class ClanModelLoader
    {
        public static List<ClanModel> LoadModels()
        {
            return new List<ClanModel>()
            {
                new(1, "Alpha", "A", 0),
                new(2, "Beta", "B", 0),
            };
        }
    }
}