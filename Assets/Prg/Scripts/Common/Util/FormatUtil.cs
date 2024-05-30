using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Formatting utilities mainly for debugging.
    /// </summary>
    /// <remarks>
    /// Note that JSON methods require "com.unity.nuget.newtonsoft-json@3.2.1" package or later
    /// to be installed via Package Manager.<br />
    /// https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM
    /// </remarks>
    public static class FormatUtil
    {
        public static string PasswordToLog(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length <= 8)
            {
                return "********";
            }
            return $"{password[..2]}********{password[^2..]}";
        }

        public static string JsonToLog(JObject jObject)
        {
            return JsonToLog(jObject?.ToString() ?? "");
        }

        public static string JsonToLog(string json, string replacement = "")
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "";
            }
            return Regex.Replace(json, @"\r\n|\n\r|\r|\n", replacement);
        }

        public static string FormatInt(Vector2 vector)
        {
            return $"{vector.x:0},{vector.y:0}";
        }

        public static string FormatInt(Vector3 vector, bool isVector3 = true)
        {
            return isVector3
                ? $"{vector.x:0},{vector.y:0},{vector.z:0}"
                : $"{vector.x:0},{vector.y:0}";
        }

        public static string FormatD1(Vector3 vector, bool isVector3 = true)
        {
            return isVector3
                ? $"{vector.x:0.0},{vector.y:0.0},{vector.z:0.0}"
                : $"{vector.x:0.0},{vector.y:0.0}";
        }

        public static string FormatD2(Vector3 vector, bool isVector3 = true)
        {
            return isVector3
                ? $"{vector.x:0.00},{vector.y:0.00},{vector.z:0.00}"
                : $"{vector.x:0.00},{vector.y:0.00}";
        }

        public static string FormatSeconds(float seconds)
        {
            var duration = TimeSpan.FromSeconds(seconds);
            return duration.ToString(seconds < 3600f ? @"mm\:ss" : @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }
    }

    public static class FormatExtensions
    {
        public static string AsD0(this Vector2 vector)
        {
            return $"{vector.x:0},{vector.y:0}";
        }

        public static string AsD1(this Vector2 vector)
        {
            return $"{vector.x:0.0},{vector.y:0.0}";
        }

        public static string AsD2(this Vector2 vector)
        {
            return $"{vector.x:0.00},{vector.y:0.00}";
        }

        public static string AsV2D0(this Vector3 vector)
        {
            return $"{vector.x:0},{vector.y:0}".Replace("Infinity", "inf");
        }

        public static string AsV2D1(this Vector3 vector)
        {
            return $"{vector.x:0.0},{vector.y:0.0}".Replace("Infinity", "inf");
        }

        public static string AsV2D2(this Vector3 vector)
        {
            return $"{vector.x:0.00},{vector.y:0.00}".Replace("Infinity", "inf");
        }
    }
}
