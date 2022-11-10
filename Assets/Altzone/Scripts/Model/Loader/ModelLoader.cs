namespace Altzone.Scripts.Model.Loader
{
    /// <summary>
    /// Utility class to load all models for runtime.
    /// </summary>
    /// <remarks>
    /// WIKI: https://github.com/Alt-Org/Altzone/wiki/ModelLoader
    /// </remarks>
    internal static class ModelLoader
    {
        public static void LoadModels()
        {
            foreach (var model in CharacterModelLoader.LoadModels())
            {
                Models.Add(model, model.Name);
            }
            foreach (var model in ClanModelLoader.LoadModels())
            {
                Models.Add(model, model.Name);
            }
        }
    }
}