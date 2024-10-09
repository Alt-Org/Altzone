using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Clan
{
    public enum ClanAge
    {
        None,
        AgeTeenagers,
        AgeToddlers,
        AgeAdults,
        AgeAllAges
    }

    public enum Language
    {
        None,
        Finnish,
        Swedish,
        English
    }

    public enum Goals
    {
        None,
        Fiilistely,
        Grindaus,
        Intohimoisuus,
        Keraily
    }

    public static class ClanDataTypeConverter
    {
        public static string GetAgeText(ClanAge age)
        {
            return age switch
            {
                ClanAge.None => "Ikäryhmä",
                ClanAge.AgeTeenagers => "Teinit",
                ClanAge.AgeToddlers => "Taaperot",
                ClanAge.AgeAdults => "Aikuiset",
                ClanAge.AgeAllAges => "Kaiken ikäiset",
                _ => "",
            };
        }

        public static string GetLanguageText(Language language)
        {
            return language switch
            {
                Language.None => "Kieli / Språk / Language",
                Language.Finnish => "Suomi",
                Language.Swedish => "Svenska",
                Language.English => "English",
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
    }
}
