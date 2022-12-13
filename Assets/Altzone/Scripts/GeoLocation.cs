using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.MiniJson;
using Prg.Scripts.Common.RestApi;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts
{
    /// <summary>
    /// Query user's geolocation from current IP address using ip-api.com REST API free service and manage GDPR consent.
    /// </summary>
    /// <remarks>
    /// https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Glossary:Country_codes <br />
    /// https://gist.github.com/sandcastle/436ecb49c749942cb52adb2da169a2d4 <br />
    /// https://developers.mopub.com/publishers/unity/gdpr/ <br />
    /// google: unity3d check gdpr requirement
    /// </remarks>
    public static class GeoLocation
    {
        private const string ApiUrl = "http://ip-api.com/json";

        public enum GdprConsent
        {
            Unknown,
            Yes,
            No,
        }

        public static LocationData Data { get; private set; }

        public static bool HasData => Data != null;

        public static void Load(Action<LocationData> callback)
        {
            Data = LocationData.Load();
            if (Data != null)
            {
                callback?.Invoke(Data);
                return;
            }
            var result = LoadLocationDataAsync(callback);
            // Keep compiler happy as we ignore the result.
            Assert.IsNotNull(result);
        }

        public static void Delete()
        {
            Data = null;
        }

        [Conditional("USE_UNITY_ADS")]
        public static void SetConsentMetaData(GeoLocation.LocationData data)
        {
            // Google Play Families compliance:
            // - mixed indicates that the app is directed at mixed audiences (including children).
            var privacyValue = PlayerPrefs.GetString(PlayerPrefKeys.ConsentFamiliesPrivacyMode, "mixed");
            var metaData = new MetaData("privacy");
            metaData.Set("mode", privacyValue);
            Advertisement.SetMetaData(metaData);

            // Complying with GDPR:
            // - false indicates that the user opts out of targeted advertising.
            var gdprConsentValue = data.GdprConsent == GeoLocation.GdprConsent.Yes ? "true" : "false";
            metaData = new MetaData("gdpr");
            metaData.Set("consent", gdprConsentValue);
            Advertisement.SetMetaData(metaData);

            // COPPA compliance and contextual ads:
            // - true indicates that the user may not receive personalized ads.
            var nonBehavioralValue = data.GdprConsent == GeoLocation.GdprConsent.Yes ? "false" : "true";
            metaData = new MetaData("user");
            metaData.Set("nonbehavioral", nonBehavioralValue);
            Advertisement.SetMetaData(metaData);
        }

        private static async Task LoadLocationDataAsync(Action<LocationData> callback)
        {
            Dictionary<string, object> ParseJson(string json)
            {
                if (!(Json.Deserialize(json) is Dictionary<string, object> jsonData))
                {
                    jsonData = new Dictionary<string, object>();
                }
                return jsonData;
            }

            Data = null;
            try
            {
                var result = await RestApiServiceAsync.ExecuteRequest("GET", ApiUrl);
                if (!result.Success)
                {
                    Debug.Log($"GET {result.Message}");
                    return;
                }
                var payload = result.Payload;
                var response = ParseJson(payload);
                if (!response.ContainsKey("status")
                    || !response.TryGetValue("country", out var country)
                    || !response.TryGetValue("countryCode", out var countryCode))
                {
                    Debug.Log($"JSON ERROR {payload.Replace('\r', '.').Replace('\n', '.')}");
                    return;
                }
                Data = LocationData.Save(country.ToString(), countryCode.ToString(), GdprConsent.Unknown);
                Debug.Log($"GeoLocation {Data}");
            }
            finally
            {
                callback?.Invoke(Data);
            }
        }

        public class LocationData
        {
            public readonly string Country;
            public readonly string CountryCode;
            public readonly bool IsGdprApplicable;
            public readonly GdprConsent GdprConsent;

            private LocationData(string country, string countryCode, GdprConsent gdprConsent)
            {
                Country = country;
                CountryCode = countryCode;
                IsGdprApplicable = _IsGdprCountry(countryCode);
                // (1) If we detect that the user was in the European Economic Area, United Kingdom, or Switzerland,
                // then we consider that GDPR applies to that user for the lifetime of that application.
                // (2) We treat non-GDPR-region users as having consent
                GdprConsent = IsGdprApplicable ? gdprConsent : GdprConsent.Yes;
            }

            internal static LocationData Load()
            {
                var country = PlayerPrefs.GetString(PlayerPrefKeys.GeoIpCountry, string.Empty);
                var countryCode = PlayerPrefs.GetString(PlayerPrefKeys.GeoIpCountryCode, string.Empty);
                if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(countryCode))
                {
                    return null;
                }
                var gdprConsent = (GdprConsent)PlayerPrefs.GetInt(PlayerPrefKeys.GdprConsent, (int)GdprConsent.Unknown);
                var locationData = new LocationData(country, countryCode, gdprConsent);
                return locationData;
            }

            internal static LocationData Save(string country, string countryCode, GdprConsent gdprConsent)
            {
                var locationData = new LocationData(country, countryCode, gdprConsent);
                locationData.SaveSettings();
                return locationData;
            }

            private void SaveSettings()
            {
                PlayerPrefs.SetString(PlayerPrefKeys.GeoIpCountry, Country);
                PlayerPrefs.SetString(PlayerPrefKeys.GeoIpCountryCode, CountryCode);
                PlayerPrefs.SetInt(PlayerPrefKeys.GdprConsent, (int)GdprConsent);
            }

            private static bool _IsGdprCountry(string code)
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
                    "Poland", "PL",
                    // European Free Trade Association (EFTA):
                    "Iceland", "IS",
                    "Liechtenstein", "LI",
                    "Norway", "NO",
                    "Switzerland", "CH",
                    // United Kingdom
                    "United Kingdom", "UK"
                };
                return Array.FindIndex(countries, x => x.Equals(code)) > 0;
            }

            public override string ToString()
            {
                return $"{Country} ({CountryCode}){(IsGdprApplicable ? " (GDPR)" : "")} consent {GdprConsent}";
            }
        }
    }
}