using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prg.Scripts.Common.RestApi;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts
{
    /// <summary>
    /// Query user's geo location from current IP address using <code>ip-api.com</code> REST API free service.
    /// </summary>
    /// <remarks>
    /// See: https://gist.github.com/sandcastle/436ecb49c749942cb52adb2da169a2d4
    /// </remarks>
    public static class GeoLocation
    {
        private const string ApiUrl = "http://ip-api.com/json";
        private const string GeoIpCountryKey = "geoip.country";
        private const string GeoIpCountryCodeKey = "geoip.countryCode";

        public class LocationData
        {
            public readonly string Country;
            public readonly string CountryCode;
            public readonly bool IsGdprCountry;

            public LocationData(string country, string countryCode, bool isGdprCountry)
            {
                Country = country;
                CountryCode = countryCode;
                IsGdprCountry = isGdprCountry;
            }

            public override string ToString()
            {
                return $"{Country} ({CountryCode}){(IsGdprCountry ? " is GDPR" : "")}";
            }
        }

        public static bool HasData => Data != null;
        public static LocationData Data { get; private set; }

        public static void Load()
        {
            var country = PlayerPrefs.GetString(GeoIpCountryKey, string.Empty);
            var countryCode = PlayerPrefs.GetString(GeoIpCountryCodeKey, string.Empty);
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(countryCode))
            {
                var result = LoadAsync();
                // Keep compiler happy as we ignore the result.
                Assert.IsNotNull(result);
            }
            else
            {
                var isGdprCountry = IsGdprCountry(countryCode);
                Data = new LocationData(country, countryCode, isGdprCountry);
            }
        }

        private static async Task LoadAsync()
        {
            Dictionary<string, string> ParseJson(string json)
            {
                json = json.Replace('{', ' ').Replace('}', ' ');
                var lines = json.Split(',');
                var data = new Dictionary<string, string>();
                for (var i = 0; i < lines.Length;++i)
                {
                    var line = lines[i];
                    var tokens = line.Trim().Split(':');
                    if (tokens.Length == 2)
                    {
                        var key = tokens[0].Trim().Replace("\"", string.Empty);
                        var value = tokens[1].Trim().Replace("\"", string.Empty);
                        data.Add(key, value);
                    }
                }
                return data;
            }

            Data = null;
            var result = await RestApiServiceAsync.ExecuteRequest("GET", ApiUrl, null, null);
            if (!result.Success)
            {
                Debug.Log($"GET {result.Message}");
                return;
            }
            var payload = result.Payload;
            var response = ParseJson(payload);
            if (!response.TryGetValue("status", out var status))
            {
                Debug.Log($"GET ERROR {payload}");
                return;
            }
            if (!response.TryGetValue("country", out var country))
            {
                Debug.Log($"GET ERROR {payload}");
                return;
            }
            if (!response.TryGetValue("countryCode", out var countryCode))
            {
                Debug.Log($"GET ERROR {payload}");
                return;
            }
            PlayerPrefs.SetString(GeoIpCountryKey, country);
            PlayerPrefs.SetString(GeoIpCountryCodeKey, countryCode);
            var isGdprCountry = IsGdprCountry(countryCode);
            Data = new LocationData(country, countryCode, isGdprCountry);
            Debug.Log($"Set GeoLocation {Data}");
        }

        private static bool IsGdprCountry(string code)
        {
            // https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Glossary:Country_codes
            // - alphabetical list of countries in their national language for EU and EFTA
            // - there are some "EU candidate countries" and "Potential candidates" not listed here
            var countries = new[]
            {
                "Belgium", "BE",
                "Greece", "EL",
                "Lithuania", "LT",
                "Portugal", "PT",
                "Bulgaria", "BG",
                "Spain", "ES",
                "Luxembourg", "LU",
                "Romania", "RO",
                "Czechia", "CZ",
                "France", "FR",
                "Hungary", "HU",
                "Slovenia", "SI",
                "Denmark", "DK",
                "Croatia", "HR",
                "Malta", "MT",
                "Slovakia", "SK",
                "Germany", "DE",
                "Italy", "IT",
                "Netherlands", "NL",
                "Finland", "FI",
                "Estonia", "EE",
                "Cyprus", "CY",
                "Austria", "AT",
                "Sweden", "SE",
                "Ireland", "IE",
                "Latvia", "LV",
                "Poland", "PL"
            };
            return Array.FindIndex(countries, x => x.Equals(code)) > 0;
        }
    }
}