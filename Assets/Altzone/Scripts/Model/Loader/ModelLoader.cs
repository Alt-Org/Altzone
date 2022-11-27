namespace Altzone.Scripts.Model.Loader
{
    /// <summary>
    /// Utility class to load all static models for runtime.
    /// </summary>
    /// <remarks>
    /// Note that this class should load everything that is read-only and not included in local or cloud storage.
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