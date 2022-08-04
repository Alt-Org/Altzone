using System.Collections.Generic;

namespace Altzone.Scripts.Model
{
    public static class CharacterModelLoader
    {
        public static List<CharacterModel> GetCharacterModels()
        {
            // HAHMOT ja niiden kuvaukset (+ värit)
            // https://docs.google.com/spreadsheets/d/1GBlkKJia89lFvEspTzrq_IJ3XXfCTRDQmB4NrZs-Npo/edit#gid=0

            return new List<CharacterModel>()
            {
                LoadCharacterModel("Koulukiusaaja", Defence.Desensitisation, 3, 9, 7, 3),
                LoadCharacterModel("Vitsiniekka", Defence.Deflection, 9, 3, 3, 4),
                LoadCharacterModel("Pappi", Defence.Introjection, 5, 5, 4, 4),
                LoadCharacterModel("Taiteilija", Defence.Projection, 4, 2, 9, 5),
                LoadCharacterModel("Hodariläski", Defence.Retroflection, 3, 7, 2, 9),
                LoadCharacterModel("Älykkö", Defence.Egotism, 6, 2, 6, 5),
                LoadCharacterModel("Tytöt", Defence.Confluence, 5, 6, 2, 6)
            };
        }

        private static CharacterModel LoadCharacterModel(string name, Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            var id = (int)mainDefence;
            return new CharacterModel(id, name, mainDefence, speed, resistance, attack, defence);
        }
    }
}