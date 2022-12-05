using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// <c>StringProperty</c> is used to store Base16k 'obfuscated' string data in similar manner as Base64.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/StringProperty", fileName = "StringProperty")]
    public class StringProperty : ScriptableObject
    {
        private const string StartDelimiter = "{";
        private const string EndDelimiter = "}";

        [SerializeField] private string _propertyValue;

        public string PropertyValue
        {
            get => GetPropertyValue();
            set => _propertyValue = value;
        }

        private string GetPropertyValue()
        {
            var isCompressed = IsCompressed(_propertyValue);
            return !isCompressed
                ? _propertyValue
                : StringSerializer.Decode(GetCompressedPayload(_propertyValue));
        }

        public static bool IsCompressed(string value)
        {
            return value != null && value.StartsWith(StartDelimiter) && value.EndsWith(EndDelimiter);
        }

        public static string GetCompressedPayload(string value)
        {
            return value.Substring(1, value.Length - 2);
        }

        public static string FormatCompressedPayload(string value)
        {
            return $"{StartDelimiter}{value}{EndDelimiter}";
        }
    }
}