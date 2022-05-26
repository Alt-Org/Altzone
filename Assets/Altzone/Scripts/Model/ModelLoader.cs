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
            
            AddCharacterModel("Koulukiusaaja", Defence.Desensitisation, 1, 9, 8, 3);
            AddCharacterModel("Vitsiniekka", Defence.Deflection, 9, 3, 4, 4);
            AddCharacterModel("Pappi", Defence.Introjection, 5, 5, 5, 5);
            AddCharacterModel("Taiteilija", Defence.Projection, 3, 2, 9, 6);
            AddCharacterModel("Hodariläski", Defence.Retroflection, 2, 7, 2, 9);
            AddCharacterModel("Älykkö", Defence.Egotism, 4, 2, 7, 7);
            AddCharacterModel("Tytöt", Defence.Confluence, 7, 7, 1, 5);

            AddModel("Alpha", new ClanModel(1, "Alpha", "ALPHA"));
            AddModel("Beta", new ClanModel(2, "Beta", "BETA"));
            AddModel("Viewer", new ClanModel(9, "Viewer", "VIEW", true));
        }

        private static void AddCharacterModel(string name, Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            var id = (int)mainDefence;
            var model = new CharacterModel(id, name, mainDefence, speed, resistance, attack, defence);
            Models.Add(model, name);
        }
        
        private static void AddModel(string name, AbstractModel model)
        {
            Models.Add(model, name);
        }
    }
}