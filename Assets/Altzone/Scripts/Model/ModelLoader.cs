namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Utility class to load all models for runtime.
    /// </summary>
    public static class ModelLoader
    {
        private static bool isModelsLoaded;

        public static void LoadModels()
        {
            if (!isModelsLoaded)
            {
                LoadAndClearModels();
            }
        }

        public static void LoadAndClearModels()
        {
            isModelsLoaded = true;
            Models.Clear();

            AddModel(Defence.Desensitisation.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Deflection.ToString(), new DefenceModel((int)Defence.Deflection, Defence.Deflection));
            AddModel(Defence.Introjection.ToString(), new DefenceModel((int)Defence.Introjection, Defence.Introjection));
            AddModel(Defence.Projection.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Retroflection.ToString(), new DefenceModel((int)Defence.Retroflection, Defence.Retroflection));
            AddModel(Defence.Egotism.ToString(), new DefenceModel((int)Defence.Egotism, Defence.Egotism));
            AddModel(Defence.Confluence.ToString(), new DefenceModel((int)Defence.Confluence, Defence.Confluence));

            AddModel("Koulukiusaaja", new CharacterModel(
                (int)Defence.Desensitisation, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 6, 2));
            AddModel("Vitsiniekka", new CharacterModel(
                (int)Defence.Deflection, "Vitsiniekka", Defence.Deflection, 9, 2, 4, 5));
            AddModel("Pappi", new CharacterModel(
                (int)Defence.Introjection, "Pappi", Defence.Introjection, 5, 5, 5, 5));
            AddModel("Taiteilija", new CharacterModel(
                (int)Defence.Projection, "Taiteilija", Defence.Projection, 2, 2, 8, 8));
            AddModel("Hodariläski", new CharacterModel(
                (int)Defence.Retroflection, "Hodariläski", Defence.Retroflection, 3, 6, 2, 9));
            AddModel("Älykkö", new CharacterModel(
                (int)Defence.Egotism, "Älykkö", Defence.Egotism, 6, 2, 6, 6));
            AddModel("Tytöt", new CharacterModel(
                (int)Defence.Confluence, "Tytöt", Defence.Confluence, 6, 7, 1, 6));

            AddModel("Alpha", new ClanModel(1, "Alpha", "ALPHA"));
            AddModel("Beta", new ClanModel(2, "Beta", "BETA"));
            AddModel("Viewer", new ClanModel(9, "Viewer", "VIEW", true));
        }

        private static void AddModel(string name, AbstractModel model)
        {
            Models.Add(model, name);
        }
    }
}