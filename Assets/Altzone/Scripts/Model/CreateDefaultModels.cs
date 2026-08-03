using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Initializes game model objects to a known state for a new player (installation).
    /// </summary>
    internal static class CreateDefaultModels
    {
        internal static string FakeMongoDbId()
        {
            return Guid.NewGuid().ToString();
        }

        internal static PlayerData CreatePlayerData(string playerGuid, string clanId, int currentCustomCharacterId)
        {
            int none = (int)CharacterID.None;
            string noneStr = none.ToString();
            string[] characters = new string[3] { currentCustomCharacterId.ToString(), noneStr, noneStr }; 

            return new PlayerData(FakeMongoDbId(), clanId, currentCustomCharacterId, characters, "Player", 0, playerGuid, null);
        }

        internal static ClanData CreateClanData(string clanId, List<GameFurniture> furniture)
        {
            var clanData = new ClanData(clanId, "DemoClan", "[D]", 0);
            clanData.Inventory.Furniture = furniture != null ? CreateDefaultDebugFurniture(new ReadOnlyCollection<GameFurniture>(furniture)) : new();
            return clanData;
        }

        public static List<ClanFurniture> CreateDefaultDebugFurniture(ReadOnlyCollection<GameFurniture> baseFurniture)
        {
            List<ClanFurniture> clanFurniture = new();
            int i = 0;
            int j = 0;
            while (i < 2)
            {
                foreach(var furniture in baseFurniture)
                {
                    j++;
                    clanFurniture.Add(new ClanFurniture((10000 + j * 100 + i).ToString(), furniture.Name));
                }

                i++;
            }

            for (i = 0; i < (ServerManager.Instance.Clan != null ? ServerManager.Instance.Clan.playerCount : 1); i++)
            {
                int slotRows = 8;
                int slotColumn = 20;

                int furniture1X = UnityEngine.Random.Range(1, slotColumn - 1);
                int furniture1Y = UnityEngine.Random.Range(1, slotRows);
                int furniture2X;
                int furniture2Y;
                while (true)
                {
                    furniture2X = UnityEngine.Random.Range(0, slotColumn - 7);
                    furniture2Y = UnityEngine.Random.Range(2, slotRows);
                    if ((furniture2X >= furniture1X - 7 && furniture2X <= furniture1X + 1 && furniture2Y >= furniture1Y - 1 && furniture2Y <= furniture1Y + 2)) continue;
                    else break;
                }

                clanFurniture.Add(new ClanFurniture((10000 + 300 + 3 + i).ToString(), "Floorlamp_Taakka", furniture1X, furniture1Y, FurnitureGrid.Floor, i, false));
                clanFurniture.Add(new ClanFurniture((10000 + 100 + 3 + i).ToString(), "Sofa_Taakka", furniture2X, furniture2Y, FurnitureGrid.Floor, i, false));

            }
            return clanFurniture;
        }

        /// <summary>
        /// Character classes are permanent and immutable that can be added but never deleted after game has been published.
        /// </summary>
        /// <returns></returns>
        internal static List<CharacterClass> CreateCharacterClasses()
        {
            return new List<CharacterClass>
            {
                new(CharacterClassType.Desensitizer,     7, 3, 9, 7, 3),
                new(CharacterClassType.Trickster,        3, 9, 3, 3, 4),
                new(CharacterClassType.Obedient,         5, 5, 5, 4, 4),
                new(CharacterClassType.Projector,        4, 4, 2, 9, 5),
                new(CharacterClassType.Retroflector,     6, 3, 7, 2, 9),
                new(CharacterClassType.Confluent,        5, 5, 6, 2, 6),
                new(CharacterClassType.Intellectualizer, 4, 6, 2, 6, 5)
            };
        }

        /// <summary>
        /// [Player] custom character classes are created by the player itself (or given to the player by the game).<br />
        /// This collection could be the initial set of custom character classes the player has when game is started first time.
        /// </summary>
        /// <returns></returns>
        internal static List<CustomCharacter> CreateCustomCharacters()
        {
            return new List<CustomCharacter>
            {
                new(CharacterID.Bodybuilder, 0, 0, 0, 0, 0),
                new(CharacterID.Joker,0, 0, 0, 0, 0),
                new(CharacterID.Conman,0, 0, 0, 0, 0),
                new(CharacterID.Religious,0, 0, 0, 0, 0),
                new(CharacterID.Artist,0, 0, 0, 0, 0),
                new(CharacterID.Overeater,0, 0, 0, 0, 0),
                new(CharacterID.Alcoholic,0, 0, 0, 0, 0),
                new(CharacterID.Soulsisters,0, 0, 0, 0, 0),
                new(CharacterID.Booksmart,0, 0, 0, 0, 0)
            };
        }

        internal static List<CustomCharacter> CreateCustomCharacters(List<BaseCharacter> characters)
        {
            List<CustomCharacter> list = new();
            foreach (BaseCharacter character in characters)
            {
                list.Add(new(character));
            }
            return list;
        }

        /// <summary>
        /// Game Furniture is based from data in Google Sheets.<br />
        /// https://docs.google.com/spreadsheets/d/1GGh2WWjZDs98yrxd2sU2STUX2AyvwqZGItSqwWWpCm4/edit#gid=0
        /// </summary>
        /// <returns></returns>
        internal static List<GameFurniture> CreateGameFurniture()
        {
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");

            var gameFurniture = new List<GameFurniture>();

            gameFurniture.Add(new GameFurniture(1.ToString(), "Sofa_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 30f, 150f));
            gameFurniture.Add(new GameFurniture(2.ToString(), "Mirror_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 8f, 100f));
            gameFurniture.Add(new GameFurniture(3.ToString(), "Floorlamp_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 2.8f, 240f));
            gameFurniture.Add(new GameFurniture(4.ToString(), "Toilet_Schrodinger", FurnitureRarity.Common, FurniturePlacement.Floor, 31f, 150f));
            gameFurniture.Add(new GameFurniture(5.ToString(), "Sink_Schrodinger", FurnitureRarity.Common, FurniturePlacement.FloorByWall, 13f, 150f));
            gameFurniture.Add(new GameFurniture(6.ToString(), "Closet_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 48f, 120f));
            gameFurniture.Add(new GameFurniture(7.ToString(), "CoffeeTable_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 26f, 80f));
            gameFurniture.Add(new GameFurniture(8.ToString(), "SideTable_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 16f, 60f));
            gameFurniture.Add(new GameFurniture(9.ToString(), "ArmChair_Taakka", FurnitureRarity.Common, FurniturePlacement.Floor, 16f, 120f));
            gameFurniture.Add(new GameFurniture(10.ToString(), "Sofa_Rakkaus", FurnitureRarity.Common, FurniturePlacement.Floor, 27f, 130f));
            gameFurniture.Add(new GameFurniture(11.ToString(), "ArmChair_Rakkaus", FurnitureRarity.Common, FurniturePlacement.Floor, 13f, 100f));
            gameFurniture.Add(new GameFurniture(12.ToString(), "Closet_Rakkaus", FurnitureRarity.Common, FurniturePlacement.Floor, 45f, 130f));
            gameFurniture.Add(new GameFurniture(13.ToString(), "Chair_Neuro", FurnitureRarity.Common, FurniturePlacement.Floor, 10f, 170f));
            gameFurniture.Add(new GameFurniture(14.ToString(), "Dresser_Neuro", FurnitureRarity.Common, FurniturePlacement.Floor, 24f, 100f));
            gameFurniture.Add(new GameFurniture(15.ToString(), "Stool_Neuro", FurnitureRarity.Rare, FurniturePlacement.Floor, 4f, 40f));
            gameFurniture.Add(new GameFurniture(16.ToString(), "Mirror_Schrodinger", FurnitureRarity.Rare, FurniturePlacement.Wall, 7f, 150f));
            gameFurniture.Add(new GameFurniture(17.ToString(), "Carpet_Schrodinger", FurnitureRarity.Rare, FurniturePlacement.FloorNonblock, 6f, 150f));
            gameFurniture.Add(new GameFurniture(18.ToString(), "Clock_Neuro", FurnitureRarity.Rare, FurniturePlacement.Wall, 1f, 40f));
            gameFurniture.Add(new GameFurniture(19.ToString(), "Carpet_Rakkaus", FurnitureRarity.Rare, FurniturePlacement.FloorNonblock, 4f, 40f));
            gameFurniture.Add(new GameFurniture(20.ToString(), "Bed_Rakkaus", FurnitureRarity.Rare, FurniturePlacement.Floor, 20f, 200f));
            gameFurniture.Add(new GameFurniture(21.ToString(), "CoffeeTable_Rakkaus", FurnitureRarity.Rare, FurniturePlacement.Floor, 20f, 60f));
            gameFurniture.Add(new GameFurniture(22.ToString(), "DiningTable_Rakkaus", FurnitureRarity.Epic, FurniturePlacement.Floor, 30f, 100f));
            gameFurniture.Add(new GameFurniture(23.ToString(), "Mirror_Rakkaus", FurnitureRarity.Epic, FurniturePlacement.Wall, 10f, 170f));
            gameFurniture.Add(new GameFurniture(24.ToString(), "Chair_Polarity", FurnitureRarity.Epic, FurniturePlacement.Floor, 5f, 100f));
            gameFurniture.Add(new GameFurniture(25.ToString(), "Commode_Muistoja", FurnitureRarity.Epic, FurniturePlacement.Floor, 17f, 100f));
            gameFurniture.Add(new GameFurniture(26.ToString(), "Pictures_Muistoja", FurnitureRarity.Epic, FurniturePlacement.Wall, 2f, 60f));
            gameFurniture.Add(new GameFurniture(27.ToString(), "Sofa_Muistoja", FurnitureRarity.Epic, FurniturePlacement.Floor, 20f, 100f));
            gameFurniture.Add(new GameFurniture(28.ToString(), "Armchair_Muistoja", FurnitureRarity.Epic, FurniturePlacement.Floor, 5f, 60f));
            gameFurniture.Add(new GameFurniture(29.ToString(), "Ficus_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Floor, 3f, 120f));
            gameFurniture.Add(new GameFurniture(30.ToString(), "Flowers_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Floor, 1f, 50f));
            gameFurniture.Add(new GameFurniture(31.ToString(), "Coffeetable_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Floor, 20f, 60f));
            gameFurniture.Add(new GameFurniture(32.ToString(), "NewCarpet_Muistoja", FurnitureRarity.Antique, FurniturePlacement.FloorNonblock, 7f, 150f));
            gameFurniture.Add(new GameFurniture(33.ToString(), "OldCarpet_Muistoja", FurnitureRarity.Antique, FurniturePlacement.FloorNonblock, 7f, 150f));
            gameFurniture.Add(new GameFurniture(34.ToString(), "Drawings_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Wall, 1f, 60f));
            gameFurniture.Add(new GameFurniture(35.ToString(), "Painting_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Wall, 2f, 40f));
            gameFurniture.Add(new GameFurniture(36.ToString(), "NewWindow_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Wall, 1f, 50f));
            gameFurniture.Add(new GameFurniture(37.ToString(), "OldWindow_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Wall, 1f, 50f));
            gameFurniture.Add(new GameFurniture(38.ToString(), "ToyFox_Muistoja", FurnitureRarity.Antique, FurniturePlacement.Floor, 1f, 70f));
            gameFurniture.Add(new GameFurniture(39.ToString(), "Bookshelf_Polarity", FurnitureRarity.Antique, FurniturePlacement.Floor, 40f, 120f));
            //gameFurniture.Add(new GameFurniture("heikko pommi", "Heikko pommi", FurnitureSize.OneXOne, FurnitureSize.OneXOne, FurniturePlacement.Floor, 10f, 15f));
            //gameFurniture.Add(new GameFurniture("tuplapommi", "Tuplapommi", FurnitureSize.OneXOne, FurnitureSize.OneXOne, FurniturePlacement.Floor, 10f, 15f));
            //gameFurniture.Add(new GameFurniture("superpommi", "Superpommi", FurnitureSize.OneXOne, FurnitureSize.OneXOne, FurniturePlacement.Floor, 10f, 15f));
            return gameFurniture;
        }
    }
}
