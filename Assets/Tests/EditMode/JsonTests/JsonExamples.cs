using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Prg.Scripts.Common.MiniJson;
using UnityEngine;

namespace Tests.EditMode.JsonTests
{
    internal class JsonTestData
    {
        public int IntNumber;
        public long LongNumber;
        public float FloatNumber;
        public double DoubleNumber;
        public string StringData;
        public string[] StringArray = Array.Empty<string>();
        public List<string> StringList = new();

        public override string ToString()
        {
            return $"{nameof(IntNumber)}: {IntNumber}, {nameof(LongNumber)}: {LongNumber}" +
                   $", {nameof(FloatNumber)}: {FloatNumber}, {nameof(DoubleNumber)}: {DoubleNumber}" +
                   $", {nameof(StringData)}: {StringData}" +
                   $", {nameof(StringArray)}: {string.Join('|', StringArray)}" +
                   $", {nameof(StringList)}: {string.Join('|', StringList)}";
        }
    }

    [TestFixture, Description("Examples how to use MiniJson and JsonUtility classes")]
    public class JsonExamples
    {
        [Test, Description("MiniJson -> convert JSON text to Dictionary<string, object>")]
        public void MiniJsonDictionaryTests()
        {
            Debug.Log("test");
            const string jsonText =
                "{\"IntNumber\":0,\"LongNumber\":0,\"FloatNumber\":0.0,\"DoubleNumber\":0.0,\"StringData\":\"\",\"StringArray\":[],\"StringList\":[]}";
            Debug.Log($"jsonText {jsonText}");
            var resultObject = MiniJson.Deserialize(jsonText);
            Debug.Log($"resultObject {resultObject}");
            var isDictionary = resultObject is Dictionary<string, object>;
            Assert.IsTrue(isDictionary);
            var resultDictionary = resultObject as Dictionary<string, object>;
            Assert.IsNotNull(resultDictionary);
            var fieldNames = typeof(JsonTestData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var fieldCount = 0;
            foreach (var fieldName in fieldNames)
            {
                Assert.IsTrue(resultDictionary.ContainsKey(fieldName.Name));
                fieldCount += 1;
            }
            Assert.AreEqual(fieldNames.Length, fieldCount);
            // JSON types converted by JSON specification
            var intValue = resultDictionary[nameof(JsonTestData.IntNumber)];
            Assert.AreEqual(typeof(long), intValue.GetType());
            var floatValue = resultDictionary[nameof(JsonTestData.FloatNumber)];
            Assert.AreEqual(typeof(double), floatValue.GetType());
            // JSON types converted by MiniJson to Dictionary
            var stringArray = resultDictionary[nameof(JsonTestData.StringArray)];
            Assert.AreEqual(typeof(List<object>), stringArray.GetType());
        }

        [Test, Description("JsonUtility -> convert C# object to JSON text and vice versa")]
        public void JsonUtilityObjectTests()
        {
            Debug.Log("test");
            var defaultData = new JsonTestData();
            Debug.Log($"defaultData {defaultData}");
            var defaultJson = JsonUtility.ToJson(defaultData);
            Debug.Log($"defaultJson {defaultJson}");
            var defaultResult = JsonUtility.FromJson<JsonTestData>(defaultJson);
            Debug.Log($"defaultResult {defaultResult}");
            Assert.AreEqual(defaultData.ToString(), defaultResult.ToString());
        }
    }
}