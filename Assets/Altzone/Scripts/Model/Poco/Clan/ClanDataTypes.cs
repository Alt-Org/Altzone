using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Clan
{
    public enum ClanValues
    {
        Elainrakkaat,
        Maahanmuuttomyonteiset,
        Lgbtq,
        Raittiit,
        Kohteliaat,
        Kiusaamisenvastaiset,
        Urheilevat,
        Syvalliset,
        Oikeudenmukaiset,
        Kaikkienkaverit,
        Itsenaiset,
        Retkeilijat,
        Suomenruotsalaiset,
        Huumorintajuiset,
        Rikkaat,
        Ikiteinit,
        Juoruilevat,
        Rakastavat,
        Oleilijat,
        Nortit,
        Musadiggarit,
        Tunteelliset,
        Gamerit,
        Animefanit,
        Sinkut,
        Monikulttuuriset,
        Kauniit,
        Jarjestelmalliset,
        Epajarjestelmalliset,
        Tasaarvoiset,
        Somepersoonat,
        Kadentaitajat,
        Muusikot,
        Taiteilijat,
        Spammaajat,
        Kasvissyojat,
        Tasapainoiset,
    }

    public class HeartPieceData
    {
        public int pieceNumber;
        public Color pieceColor;

        public HeartPieceData(int number, Color color)
        {
            pieceNumber = number;
            pieceColor = color;
        }
    }

    public enum ClanAge
    {
        None,
        Teenagers,
        Toddlers,
        Adults,
        All
    }

    public enum Language
    {
        None,
        Finnish,
        Swedish,
        English,
        Spanish,
        Russian,
        Ukrainian
    }

    public enum Goals
    {
        None,
        Fiilistely,
        Grindaus,
        Intohimoisuus,
        Keraily
    }

    public enum ClanActivity
    {
        None,
        VeryActive,
        Active,
        OccasionallyActive,
        RarelyActive
    }

    public static class ClanDataTypeConverter
    {
        public static string GetAgeText(ClanAge age)
        {
            return age switch
            {
                ClanAge.None => "Ikäryhmä",
                ClanAge.Teenagers => "Teinit",
                ClanAge.Toddlers => "Taaperot",
                ClanAge.Adults => "Aikuiset",
                ClanAge.All => "Kaiken ikäiset",
                _ => "",
            };
        }

        public static string GetLanguageText(Language language)
        {
            return language switch
            {
                Language.None => "Kieli / Språk / Language",
                Language.Finnish => "suomi",
                Language.Swedish => "svenska",
                Language.English => "English",
                Language.Spanish => "espanol",
                Language.Russian => "venäjä",
                Language.Ukrainian => "ukraina",
                _ => "",
            };
        }

        public static string GetGoalText(Goals goal)
        {
            return goal switch
            {
                Goals.None => "Tavoite",
                Goals.Fiilistely => "Fiilistely",
                Goals.Grindaus => "Grindaus",
                Goals.Intohimoisuus => "Intohimoisuus",
                Goals.Keraily => "Keraily",
                _ => "",
            };
        }

        public static string GetActivityText(ClanActivity activity)
        {
            return activity switch
            {
                ClanActivity.None => "Aktiivisuusluokka",
                ClanActivity.VeryActive => "Erittäin aktiivinen",
                ClanActivity.Active => "Aktiivinen",
                ClanActivity.OccasionallyActive => "Satunnainen",
                ClanActivity.RarelyActive => "Harvoin paikalla",
                _ => "",
            };
        }
    }
}
