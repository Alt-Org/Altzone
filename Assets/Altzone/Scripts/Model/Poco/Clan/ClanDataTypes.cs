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
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return age switch
                    {
                        ClanAge.None => "Ikäryhmä",
                        ClanAge.Teenagers => "Teinit",
                        ClanAge.Toddlers => "Taaperot",
                        ClanAge.Adults => "Aikuiset",
                        ClanAge.All => "Kaikki",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return age switch
                    {
                        ClanAge.None => "Age Group",
                        ClanAge.Teenagers => "Teenagers",
                        ClanAge.Toddlers => "Toddlers",
                        ClanAge.Adults => "Adults",
                        ClanAge.All => "Everyone",
                        _ => "",
                    };
            }
            return age switch
            {
                ClanAge.None => "Ikäryhmä",
                ClanAge.Teenagers => "Teinit",
                ClanAge.Toddlers => "Taaperot",
                ClanAge.Adults => "Aikuiset",
                ClanAge.All => "Kaikki",
                _ => "",
            };
        }


        public static string GetLanguageText(Language language)
        {
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return language switch
                    {
                        Language.None => "Kieli",
                        Language.Finnish => "suomi",
                        Language.Swedish => "svenska",
                        Language.English => "English",
                        Language.Spanish => "espanol",
                        Language.Russian => "venäjä",
                        Language.Ukrainian => "ukraina",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return language switch
                    {
                        Language.None => "Language",
                        Language.Finnish => "suomi",
                        Language.Swedish => "svenska",
                        Language.English => "English",
                        Language.Spanish => "espanol",
                        Language.Russian => "russian",
                        Language.Ukrainian => "ukrainian",
                        _ => "",
                    };
            }
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
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return goal switch
                    {
                        Goals.None => "Tavoite",
                        Goals.Fiilistely => "Fiilistely",
                        Goals.Grindaus => "Grindaus",
                        Goals.Intohimoisuus => "Intohimoisuus",
                        Goals.Keraily => "Keraily",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return goal switch
                    {
                        Goals.None => "Goals",
                        Goals.Fiilistely => "Vibing",
                        Goals.Grindaus => "Grind",
                        Goals.Intohimoisuus => "Passion",
                        Goals.Keraily => "Collection",
                        _ => "",
                    };
            }
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
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return activity switch
                    {
                        ClanActivity.None => "Aktiivisuus",
                        ClanActivity.VeryActive => "Erittäin aktiivinen",
                        ClanActivity.Active => "Aktiivinen",
                        ClanActivity.OccasionallyActive => "Satunnainen",
                        ClanActivity.RarelyActive => "Harvoin paikalla",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return activity switch
                    {
                        ClanActivity.None => "Activity",
                        ClanActivity.VeryActive => "Very Active",
                        ClanActivity.Active => "Active",
                        ClanActivity.OccasionallyActive => "Occasionally Active",
                        ClanActivity.RarelyActive => "Rarely Active",
                        _ => "",
                    };
            }
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
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return ranking switch
                    {
                        ClanRanking.None => "Sijoitus",
                        ClanRanking.Top10 => "Top 10",
                        ClanRanking.Top25 => "Top 25",
                        ClanRanking.Top50 => "Top 50",
                        ClanRanking.Top100 => "Top 100",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return ranking switch
                    {
                        ClanRanking.None => "Ranking",
                        ClanRanking.Top10 => "Top 10",
                        ClanRanking.Top25 => "Top 25",
                        ClanRanking.Top50 => "Top 50",
                        ClanRanking.Top100 => "Top 100",
                        _ => "",
                    };
            }
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
            switch (SettingsCarrier.Instance.Language)
            {
                case SettingsCarrier.LanguageType.Finnish:
                    return members switch
                    {
                        ClanMembers.None => "Jäsenmäärä",
                        ClanMembers.Small => "Pieni (1-5)",
                        ClanMembers.Medium => "Keskikokoinen (6-10)",
                        ClanMembers.Large => "Suuri (11-20)",
                        ClanMembers.VeryLarge => "Hyvin suuri (21-28)",
                        ClanMembers.Huge => "Valtava (29+)",
                        _ => "",
                    };
                case SettingsCarrier.LanguageType.English:
                    return members switch
                    {
                        ClanMembers.None => "Member count",
                        ClanMembers.Small => "Small (1-5)",
                        ClanMembers.Medium => "Medium (6-10)",
                        ClanMembers.Large => "Large (11-20)",
                        ClanMembers.VeryLarge => "Very Large (21-28)",
                        ClanMembers.Huge => "Huge (29+)",
                        _ => "",
                    };
            }
            return members switch
            {
                ClanMembers.None => "Jäsenmäärä",
                ClanMembers.Small => "Pieni (1-5)",
                ClanMembers.Medium => "Keskikokoinen (6-10)",
                ClanMembers.Large => "Suuri (11-20)",
                ClanMembers.VeryLarge => "Hyvin suuri (21-28)",
                ClanMembers.Huge => "Valtava (29+)",
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
