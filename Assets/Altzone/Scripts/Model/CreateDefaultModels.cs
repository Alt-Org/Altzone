using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Altzone.Scripts.Model.Poco.Game;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Initializes game model objects to a known state for a new player (installation).
    /// </summary>
    internal static class CreateDefaultModels
    {
        internal const int CharacterClassesVersion = 1;

        /// <summary>
        /// Character classes are permanent and immutable that can be added but never deleted after game has been published.
        /// </summary>
        /// <returns></returns>
        internal static List<CharacterClass> CreateCharacterClasses()
        {
            return new List<CharacterClass>
            {
                new(1, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 7, 3),
                new(2, "Vitsiniekka", Defence.Deflection, 9, 3, 3, 4),
                new(3, "Pappi", Defence.Introjection, 5, 5, 4, 4),
                new(4, "Taiteilija", Defence.Projection, 4, 2, 9, 5),
                new(5, "Hodariläski", Defence.Retroflection, 3, 7, 2, 9),
                new(6, "Älykkö", Defence.Egotism, 6, 2, 6, 5),
                new(7, "Tytöt", Defence.Confluence, 5, 6, 2, 6)
            };
        }

        internal const int CustomCharactersVersion = 4;

        /// <summary>
        /// Player custom character classes are created by the player itself (or given to the player by the game).<br />
        /// This collection should be the initial set of custom character classes the player has when game is started first time.
        /// </summary>
        /// <returns></returns>
        internal static List<CustomCharacter> CreateCustomCharacters()
        {
            return new List<CustomCharacter>
            {
                new(1, 1, "1", "<1>", 0, 0, 0, 0),
                new(2, 2, "2", "<2>", 0, 0, 0, 0),
                new(3, 3, "3", "<3>", 0, 0, 0, 0),
                new(4, 4, "4", "<4>", 0, 0, 0, 0),
                new(5, 5, "5", "<5>", 0, 0, 0, 0),
                new(6, 6, "6", "<6>", 0, 0, 0, 0),
                new(7, 7, "7", "<7>", 0, 0, 0, 06)
            };
        }

        internal const int GameFurnitureVersion = 2;

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
                    Id = ParseInt(tokens[0]),
                    Name = tokens[1],
                    Shape = tokens[2],
                    Weight = ParseDouble(tokens[3]),
                    Material = tokens[4],
                    Recycling = tokens[5],
                    UnityKey = tokens[6],
                    Filename = tokens[7],
                };
                if (furniture.Id == 0)
                {
                    furniture.Id = gameFurniture.Count + 1;
                }
                if (string.IsNullOrWhiteSpace(furniture.Name))
                {
                    furniture.Name = $"Furniture-{furniture.Id}";
                }
                Debug.Log(furniture.ToString());
                gameFurniture.Add(furniture);
            }
            return gameFurniture;

            int ParseInt(string token)
            {
                if (token.Contains(','))
                {
                    token = token.Replace(',', '.');
                }
                if (int.TryParse(token, NumberStyles.None, cultureInfo, out var number))
                {
                    return number;
                }
                return 0;
            }

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