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

            add(Defence.Desensitisation.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            add(Defence.Deflection.ToString(), new DefenceModel((int)Defence.Deflection, Defence.Deflection));
            add(Defence.Introjection.ToString(), new DefenceModel((int)Defence.Introjection, Defence.Introjection));
            add(Defence.Projection.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            add(Defence.Retroflection.ToString(), new DefenceModel((int)Defence.Retroflection, Defence.Retroflection));
            add(Defence.Egotism.ToString(), new DefenceModel((int)Defence.Egotism, Defence.Egotism));
            add(Defence.Confluence.ToString(), new DefenceModel((int)Defence.Confluence, Defence.Confluence));

            add("Koulukiusaaja", new CharacterModel(
                (int)Defence.Desensitisation, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 6, 2));
            add("Vitsiniekka", new CharacterModel(
                (int)Defence.Deflection, "Vitsiniekka", Defence.Deflection, 9, 2, 4, 5));
            add("Pappi", new CharacterModel(
                (int)Defence.Introjection, "Pappi", Defence.Introjection, 5, 5, 5, 5));
            add("Taiteilija", new CharacterModel(
                (int)Defence.Projection, "Taiteilija", Defence.Projection, 2, 2, 8, 8));
            add("Hodariläski", new CharacterModel(
                (int)Defence.Retroflection, "Hodariläski", Defence.Retroflection, 3, 6, 2, 9));
            add("Älykkö", new CharacterModel(
                (int)Defence.Egotism, "Älykkö", Defence.Egotism, 6, 2, 6, 6));
            add("Tytöt", new CharacterModel(
                (int)Defence.Confluence, "Tytöt", Defence.Confluence, 6, 7, 1, 6));
        }

        private static void add(string name, AbstractModel model)
        {
            Models.Add(model, name);
        }
    }
}