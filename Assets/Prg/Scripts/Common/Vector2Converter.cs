using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

public class Vector2Converter : JsonConverter
{
    public Vector2Converter()
    {
    }

    public override bool CanConvert(Type objectType) => typeof(Vector2).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            var token = JToken.ReadFrom(reader);

            if (token is not JArray asArray)
            {
                throw new JsonReaderException($"Could not read {objectType} from json, expected a json array but got {token.Type}");
            }

            if (asArray.Count < 2)
            {
                throw new JsonReaderException($"Could not read {typeof(Vector2)} from json, expected a json array with 2 elements");
            }
            return new Vector2(
                asArray[0].ToObject<float>(),
                asArray[1].ToObject<float>());

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse {objectType} : {ex.Message}");
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {

        if (value is not Vector2 asVector2)
        {
            throw new JsonReaderException();
        }
            
        writer.WriteStartArray();
        writer.WriteValue(asVector2.x);
        writer.WriteValue(asVector2.y);
        writer.WriteEndArray();

    }
}
