namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Utility class to load all models for runtime.
    /// </summary>
    public static class ModelLoader
    {
        private static bool _isModelsLoaded;

        public static void LoadModels()
        {
            if (!_isModelsLoaded)
            {
                LoadAndClearModels();
            }
        }

        public static void LoadAndClearModels()
        {
            _isModelsLoaded = true;
            Models.Clear();

            AddModel(Defence.Desensitisation.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Deflection.ToString(), new DefenceModel((int)Defence.Deflection, Defence.Deflection));
            AddModel(Defence.Introjection.ToString(), new DefenceModel((int)Defence.Introjection, Defence.Introjection));
            AddModel(Defence.Projection.ToString(), new DefenceModel((int)Defence.Desensitisation, Defence.Desensitisation));
            AddModel(Defence.Retroflection.ToString(), new DefenceModel((int)Defence.Retroflection, Defence.Retroflection));
            AddModel(Defence.Egotism.ToString(), new DefenceModel((int)Defence.Egotism, Defence.Egotism));
            AddModel(Defence.Confluence.ToString(), new DefenceModel((int)Defence.Confluence, Defence.Confluence));

            // HAHMOT ja niiden kuvaukset (+ värit)
            // https://docs.google.com/spreadsheets/d/1GBlkKJia89lFvEspTzrq_IJ3XXfCTRDQmB4NrZs-Npo/edit#gid=0

            // Last edit was made on 18 January, 17:24 Helena Pavloff-Pelkonen
            
            AddModel("Koulukiusaaja", new CharacterModel(
                (int)Defence.Desensitisation, "Koulukiusaaja", Defence.Desensitisation, 1, 9, 8, 3));
            AddModel("Vitsiniekka", new CharacterModel(
                (int)Defence.Deflection, "Vitsiniekka", Defence.Deflection, 9, 3, 4, 4));
            AddModel("Pappi", new CharacterModel(
                (int)Defence.Introjection, "Pappi", Defence.Introjection, 5, 5, 5, 5));
            AddModel("Taiteilija", new CharacterModel(
                (int)Defence.Projection, "Taiteilija", Defence.Projection, 3, 2, 9, 6));
            AddModel("Hodariläski", new CharacterModel(
                (int)Defence.Retroflection, "Hodariläski", Defence.Retroflection, 2, 7, 2, 9));
            AddModel("Älykkö", new CharacterModel(
                (int)Defence.Egotism, "Älykkö", Defence.Egotism, 4, 2, 7, 7));
            AddModel("Tytöt", new CharacterModel(
                (int)Defence.Confluence, "Tytöt", Defence.Confluence, 7, 7, 1, 5));

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