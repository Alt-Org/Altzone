using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourConverter : JsonConverter
{
    public ColourConverter()
    {
    }

    public override bool CanConvert(Type objectType) => typeof(Color).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            ColorUtility.TryParseHtmlString(reader.Value.ToString(), out Color loadedColor);
            return loadedColor;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse colour {objectType} : {ex.Message}");
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        string val = ColorUtility.ToHtmlStringRGB((Color)value);
        writer.WriteValue("#" + val);
    }
}
