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
        Adults
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

    public enum ClanRanking
    {
        None,
        Top10,
        Top25,
        Top50,
        Top100
    }

    public enum ClanMembers
    {
        None,
        Small,      
        Medium,     
        Large,      
        VeryLarge,  
        Huge
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
                ClanActivity.None => "Aktiivisuus",
                ClanActivity.VeryActive => "Erittäin aktiivinen",
                ClanActivity.Active => "Aktiivinen",
                ClanActivity.OccasionallyActive => "Satunnainen",
                ClanActivity.RarelyActive => "Harvoin paikalla",
                _ => "",
            };
        }

        public static string GetRankingText(ClanRanking ranking)
        {
            return ranking switch
            {
                ClanRanking.None => "Sijoitus",
                ClanRanking.Top10 => "Top 10",
                ClanRanking.Top25 => "Top 25",
                ClanRanking.Top50 => "Top 50",
                ClanRanking.Top100 => "Top 100",
                _ => "",
            };
        }

        public static string GetMembersText(ClanMembers members)
        {
            return members switch
            {
                ClanMembers.None => "Jäsenmäärä",
                ClanMembers.Small => "Pieni (1-10)",
                ClanMembers.Medium => "Keskikokoinen (11-25)",
                ClanMembers.Large => "Suuri (26-50)",
                ClanMembers.VeryLarge => "Hyvin suuri (51-100)",
                ClanMembers.Huge => "Valtava (100+)",
                _ => "",
            };
        }

        public static string ClanValuesToString(ClanValues value)
        {
            return value switch
            {
                ClanValues.Elainrakkaat => "eläinrakkaat",
                ClanValues.Maahanmuuttomyonteiset => "maahanmuuttomyönteiset",
                ClanValues.Lgbtq => "lgbtq+",
                ClanValues.Raittiit => "raittiit",
                ClanValues.Kohteliaat => "kohteliaat",
                ClanValues.Kiusaamisenvastaiset => "kiusaamisenvastaiset",
                ClanValues.Urheilevat => "urheilevat",
                ClanValues.Syvalliset => "syvälliset",
                ClanValues.Oikeudenmukaiset => "oikeudenmukaiset",
                ClanValues.Kaikkienkaverit => "kaikkien kaverit",
                ClanValues.Itsenaiset => "itsenäiset",
                ClanValues.Retkeilijat => "retkeilijät",
                ClanValues.Suomenruotsalaiset => "suomenruotsalaiset",
                ClanValues.Huumorintajuiset => "huumorintajuiset",
                ClanValues.Rikkaat => "rikkaat",
                ClanValues.Ikiteinit => "ikiteinit",
                ClanValues.Juoruilevat => "juoruilevat",
                ClanValues.Rakastavat => "rakastavat",
                ClanValues.Oleilijat => "oleilijat",
                ClanValues.Nortit => "nörtit",
                ClanValues.Musadiggarit => "musadiggarit",
                ClanValues.Tunteelliset => "tunteelliset",
                ClanValues.Gamerit => "gamerit",
                ClanValues.Animefanit => "animefanit",
                ClanValues.Sinkut => "sinkut",
                ClanValues.Monikulttuuriset => "monikulttuuriset",
                ClanValues.Kauniit => "kauniit",
                ClanValues.Jarjestelmalliset => "järjestelmälliset",
                ClanValues.Epajarjestelmalliset => "epäjärjestelmälliset",
                ClanValues.Tasaarvoiset => "tasa-arvoiset",
                ClanValues.Somepersoonat => "somepersoonat",
                ClanValues.Kadentaitajat => "kädentaitajat",
                ClanValues.Muusikot => "muusikot",
                ClanValues.Taiteilijat => "taiteilijat",
                ClanValues.Spammaajat => "spämmääjät",
                ClanValues.Kasvissyojat => "kasvissyöjät",
                ClanValues.Tasapainoiset => "tasapainoiset",
                _ => "",
            };
        }
    }
}
