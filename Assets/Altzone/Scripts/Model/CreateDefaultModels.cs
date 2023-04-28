using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Initializes game model objects to a known state for a new player (installation).
    /// </summary>
    /// <remarks>
    /// Note that 'data ownership' is tagged with [Player] or [Game] to indicate its relation to for example future data updates.
    /// </remarks>
    internal static class CreateDefaultModels
    {
        /// <summary>
        /// Master version number for storage data.<br />
        /// Changing this will force full storage reset, what ever this means!<br />
        /// For local data this means it could be deleted and re-created to get rid of all unwanted or obsoleted data.<br />
        /// You can change this for example if some data models has been changed that are not backwards compatible. 
        /// </summary>
        internal const int MasterStorageVersionNumber = 3;

        /// <summary>
        /// [Player] <c>PlayerData</c> version number for data update purposes.
        /// </summary>
        internal const int PlayerDataVersion = 2;

        internal static PlayerData CreatePlayerData(string playerGuid, string clanId, int currentCustomCharacterId)
        {
            return new PlayerData(0, clanId, currentCustomCharacterId, "Player", 0, playerGuid);
        }

        /// <summary>
        /// [Player] <c>ClanData</c> version number for data update purposes.
        /// </summary>
        internal const int ClanDataVersion = 5;

        internal static ClanData CreateClanData(string clanId, ReadOnlyCollection<GameFurniture> furniture)
        {
            var fakeFurnitureCounter = 0;

            string FakeFurnitureId(string furnitureText)
            {
                return $"{furnitureText[0]}-{++fakeFurnitureCounter}";
            }

            var clanData = new ClanData(clanId, "DemoClan", "[D]", 0);
            // Add every known furniture to clan inventory for testing.
            foreach (var gameFurniture in furniture)
            {
                if (gameFurniture.Id.Contains("pommi"))
                {
                    continue;
                }
                clanData.Inventory.Furniture.Add(new ClanFurniture(FakeFurnitureId(gameFurniture.Id), gameFurniture.Id));
            }
            var chairs = clanData.Inventory.Furniture.Where(x => x.GameFurnitureId.Contains("tuoli")).ToList();
            var tables = clanData.Inventory.Furniture.Where(x => x.GameFurnitureId.Contains("pöytä")).ToList();
            var misc = clanData.Inventory.Furniture.Where(x => x.GameFurnitureId.EndsWith("y")).ToList();

            // Note that bombs are not saved with other furniture because there is no specification how bombs are handled in game :-(
            var bombs = furniture.Where(x => x.Id.Contains("pommi")).ToList();
            var bomb1 = new ClanFurniture(FakeFurnitureId(bombs[0].Id), bombs[0].Id);
            var bomb2 = new ClanFurniture(FakeFurnitureId(bombs[bombs.Count - 1].Id), bombs[bombs.Count - 1].Id);

            // Create some Raid game rooms for testing.
            const int rowCount = 9;
            const int colCount = 9;
            const int rowByMemberCount = 3;
            const int colByMemberCount = 3;
            {
                var raidRoom = new RaidRoom(1, 0, RaidRoomType.Public,
                    rowCount + 1 * rowByMemberCount, colCount + 1 * colByMemberCount);
                clanData.Rooms.Add(raidRoom);
                var roomFurniture = raidRoom.Furniture;

                roomFurniture.Add(new RaidRoomFurniture(1, bomb1.GameFurnitureId, 0, 0));
                roomFurniture.Add(new RaidRoomFurniture(1, bomb2.GameFurnitureId, raidRoom.RowCount - 1, raidRoom.ColCount - 1));

                roomFurniture.Add(new RaidRoomFurniture(1, chairs[0].GameFurnitureId, 1, 1));
                roomFurniture.Add(new RaidRoomFurniture(1, tables[0].GameFurnitureId, 2, 2));
                roomFurniture.Add(new RaidRoomFurniture(1, misc[0].GameFurnitureId, 3, 3));
            }
            {
                var raidRoom = new RaidRoom(2, 0, RaidRoomType.Public,
                    rowCount + 3 * rowByMemberCount, colCount + 3 * colByMemberCount);
                clanData.Rooms.Add(raidRoom);
                var roomFurniture = raidRoom.Furniture;

                roomFurniture.Add(new RaidRoomFurniture(1, bomb1.GameFurnitureId, 0, raidRoom.ColCount - 1));
                roomFurniture.Add(new RaidRoomFurniture(1, bomb2.GameFurnitureId, raidRoom.RowCount - 1, 0));

                var row = 0;
                var col = 0;
                foreach (var item in chairs)
                {
                    col += 2;
                    roomFurniture.Add(new RaidRoomFurniture(1, item.GameFurnitureId, row, col));
                }
                row += 2;
                col = 0;
                foreach (var item in tables)
                {
                    col += 2;
                    roomFurniture.Add(new RaidRoomFurniture(1, item.GameFurnitureId, row, ++col));
                }
                row += 2;
                col = 0;
                foreach (var item in misc)
                {
                    col += 2;
                    roomFurniture.Add(new RaidRoomFurniture(1, item.GameFurnitureId, row, ++col));
                }
            }
            return clanData;
        }

        /// <summary>
        /// [Game] <c>CharacterClass</c> version number for data update purposes.
        /// </summary>
        internal const int CharacterClassesVersion = 4;

        /// <summary>
        /// Character classes are permanent and immutable that can be added but never deleted after game has been published.
        /// </summary>
        /// <returns></returns>
        internal static List<CharacterClass> CreateCharacterClasses()
        {
            return new List<CharacterClass>
            {
                new((int)GestaltCycle.Desensitisation, GestaltCycle.Desensitisation, "Koulukiusaaja", 3, 9, 7, 3),
                new((int)GestaltCycle.Deflection, GestaltCycle.Deflection, "Vitsiniekka", 9, 3, 3, 4),
                new((int)GestaltCycle.Introjection, GestaltCycle.Introjection, "Pappi", 5, 5, 4, 4),
                new((int)GestaltCycle.Projection, GestaltCycle.Projection, "Taiteilija", 4, 2, 9, 5),
                new((int)GestaltCycle.Retroflection, GestaltCycle.Retroflection, "Hodariläski", 3, 7, 2, 9),
                new((int)GestaltCycle.Egotism, GestaltCycle.Egotism, "Älykkö", 6, 2, 6, 5),
                new((int)GestaltCycle.Confluence, GestaltCycle.Confluence, "Tytöt", 5, 6, 2, 6)
            };
        }

        /// <summary>
        /// [Player] <c>CustomCharacter</c> version number for data update purposes.
        /// </summary>
        internal const int CustomCharactersVersion = 5;

        /// <summary>
        /// [Player] custom character classes are created by the player itself (or given to the player by the game).<br />
        /// This collection should be the initial set of custom character classes the player has when game is started first time.
        /// </summary>
        /// <returns></returns>
        internal static List<CustomCharacter> CreateCustomCharacters()
        {
            return new List<CustomCharacter>
            {
                new(1, 1, "1", "Keijo Kelmi", 0, 0, 0, 0),
                new(2, 2, "2", "Huugo Hupaisa", 0, 0, 0, 0),
                new(3, 3, "3", "Paavali Pappila", 0, 0, 0, 0),
                new(4, 4, "4", "Tarmo Taide", 0, 0, 0, 0),
                new(5, 5, "5", "Poju Pullukka", 0, 0, 0, 0),
                new(6, 6, "6", "Albert Älypää", 0, 0, 0, 0),
                new(7, 7, "7", "Tiina & Tuula Tyllerö", 0, 0, 0, 06)
            };
        }

        /// <summary>
        /// [Game] <c>GameFurniture</c> version number for data update purposes.
        /// </summary>
        internal const int GameFurnitureVersion = 3;

        /// <summary>
        /// Game Furniture is based from data in Google Sheets.<br />
        /// https://docs.google.com/spreadsheets/d/1GGh2WWjZDs98yrxd2sU2STUX2AyvwqZGItSqwWWpCm4/edit#gid=0
        /// </summary>
        /// <returns></returns>
        internal static List<GameFurniture> CreateGameFurniture()
        {
            const char separator = '\t';
            const int columnCount = 8;
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");

            var gameFurniture = new List<GameFurniture>();
            using var reader = new StringReader(FurnitureTsvData);
            // Skip header!
            reader.ReadLine();
            for (;;)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var tokens = line.Split(separator);
                if (tokens.Length < columnCount)
                {
                    Debug.Log($"Line is too short: {line.Replace(separator, '|')}");
                    continue;
                }
                var furniture = new GameFurniture
                {
                    Id = tokens[0],
                    Name = tokens[1],
                    Shape = tokens[2],
                    Weight = ParseDouble(tokens[3]),
                    Material = tokens[4],
                    Recycling = tokens[5],
                    UnityKey = tokens[6],
                    Filename = tokens[7],
                };
                if (string.IsNullOrWhiteSpace(furniture.Name))
                {
                    furniture.Name = $"Furniture-{101 + gameFurniture.Count}";
                }
                if (string.IsNullOrWhiteSpace(furniture.Id))
                {
                    furniture.Id = furniture.Name;
                }
                furniture.Id = furniture.Id.Trim().ToLower(cultureInfo).Replace(" ", ".");
                gameFurniture.Add(furniture);
            }
            return gameFurniture;

            double ParseDouble(string token)
            {
                if (token.Contains(','))
                {
                    token = token.Replace(',', '.');
                }
                if (double.TryParse(token, NumberStyles.AllowDecimalPoint, cultureInfo, out var number))
                {
                    return number;
                }
                return 0;
            }
        }

        private static string FurnitureTsvData = @"ID	huonekalun nimi	muoto	paino / kg	materiaali	kierrätys	prefabin nimi	tiedoston nimi	kuva					
	HUONEKASVI	OneSquare	5,2	kasvi	biojäte		huonekasvi2_elias.png			muotovaihtoehdot:	muoto	koko	lkm
	ROSKAKORI	OneSquare	2,3	kovamuovi	energiajäte					OneSquare	■	1	10
	RUOKATUOLI	OneSquare	6,6	puu	puujäte					TwoSquare	■■	2	6
	NOJATUOLI	OneSquare	35	verhoiltu	verhoillut huonekalut					StraightThreeSquares	■■■	3	5
	KORISTEPATSAS	OneSquare	5,75	kipsi	kipsijäte					BendThreeSquares	■■ ■	3	3
	NALLE	OneSquare	0,3	verhoiltu	energiajäte					FourSquares	■■ ■■	4	4
	HEIKKO POMMI	OneSquare	10	sisältää ruutia	poliisi								
	JÄÄKAAPPI	OneSquare	50	sähkölaite	sähkölaitteet					Yhteensä			28
	HELLA	OneSquare	50	sähkölaite	sähkölaitteet								
	PIENI PYÖREÄ SIVUPÖYTÄ	OneSquare	3,5	puu	puujäte					kierrätysohjeet:			
	KAHDEN ISTUTTAVA SOHVA	TwoSquare	27,6	verhoiltu	verhoillut huonekalut					Jäteopas - HSY	Sekajäte - Salpakierto		
	MATTO, OVAALI	TwoSquare	4	buklee	sekajäte								
	TUPLAPOMMI	TwoSquare	20	sisältää ruutia	poliisi					kuvatiedostot: 			
	ARKKUPAKASTIN	TwoSquare	45	sähkölaite	sähkölaitteet					HUONEKALUT			
	SOUTULAITE	TwoSquare	37	metalli, ei sisällä elektroniikkaa	metalli								
	SOHVAPÖYTÄ	TwoSquare	18,5	puu	puujäte								
	KOLMEN ISTUTTAVA SOHVA	StraightThreeSquares	71	verhoiltu	verhoillut huonekalut								
	PITKÄ RUOKAPÖYTÄ	StraightThreeSquares	61	puu	puujäte								
	KÄYTÄVÄMATTO	StraightThreeSquares	1,9	räsymatto	energiajäte								
	KEITTIÖSAAREKE	StraightThreeSquares	120	puu	puujäte								
	KIRJAHYLLY	StraightThreeSquares	100	puu	puujäte								
	TV TASO	BendThreeSquares	51	puu	puujäte								
	KOLMEN ISTUTTAVA KULMASOHVA	BendThreeSquares	96	verhoiltu	verhoillut huonekalut								
	KULMAHYLLY	BendThreeSquares	70	puu	puujäte								
	RUOKAPÖYTÄ	FourSquares	61	puu	puujäte								
	SUPERPOMMI	FourSquares	40	sisältää ruutia	poliisi								
	ISO MATTO	FourSquares	10	villamatto	energiajäte								
	PARISÄNKY	FourSquares	80	runkopatjasänky	sekajäte								
";
    }
}