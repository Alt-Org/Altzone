namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Utility class to load all models for runtime.
    /// </summary>
    internal static class ModelLoader
    {
        public static void LoadModels()
        {
            AddModel(Defence.Desensitisation.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Deflection.ToString(), new DefenceModel((int)Defence.Deflection, Defence.Deflection));
            AddModel(Defence.Introjection.ToString(), new DefenceModel((int)Defence.Introjection, Defence.Introjection));
            AddModel(Defence.Projection.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Retroflection.ToString(), new DefenceModel((int)Defence.Retroflection, Defence.Retroflection));
            AddModel(Defence.Egotism.ToString(), new DefenceModel((int)Defence.Egotism, Defence.Egotism));
            AddModel(Defence.Confluence.ToString(), new DefenceModel((int)Defence.Confluence, Defence.Confluence));

            foreach (var model in CharacterModelLoader.GetCharacterModels())
            {
                Models.Add(model, model.Name);
            }

            AddModel("Alpha", new ClanModel(1, "Alpha", "ALPHA"));
            AddModel("Beta", new ClanModel(2, "Beta", "BETA"));
            AddModel("Viewer", new ClanModel(9, "Viewer", "VIEW", true));

            void AddModel(string name, AbstractModel model)
            {
                Models.Add(model, name);
            }
        }
    }
}