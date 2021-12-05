using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Utility class to read and write objects to stream using <c>Reflection</c>.
    /// </summary>
    /// <remarks>
    /// Only non-static public fields are processed!
    /// </remarks>
    public static class BinarySerializer
    {
        /// <summary>
        /// Serializes an object instance to stream.
        /// </summary>
        public static int Serialize<T>(T instance, BinaryWriter writer) where T : class
        {
            var type = instance.GetType();
            var properties = type.GetProperties();
            Assert.AreEqual(0, properties.Length);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(x => x.Name)
                .ToList();
            Assert.IsTrue(fields.Count > 0);
            var countBytes = 0;
            foreach (var fieldInfo in fields)
            {
                var fieldTypeName = fieldInfo.FieldType.Name;
                var value = fieldInfo.GetValue(instance);
                switch (fieldTypeName)
                {
                    case "Boolean":
                        writer.Write((bool)value);
                        countBytes += 1;
                        break;
                    case "Int32":
                        writer.Write((int)value);
                        countBytes += 4;
                        break;
                    case "Single":
                        writer.Write((float)value);
                        countBytes += 4;
                        break;
                    default:
                        throw new UnityException($"unsupported field type: {fieldTypeName}");
                }
            }
            return countBytes;
        }

        /// <summary>
        /// Deserializes an object instance from stream using <c>Reflection</c>.
        /// </summary>
        /// <remarks>
        /// Only non-static public fields are processed!
        /// </remarks>
        public static Tuple<T,int> Deserialize<T>(BinaryReader reader) where T : class, new()
        {
            var instance = new T();
            var type = instance.GetType();
            var properties = type.GetProperties();
            Assert.AreEqual(0, properties.Length);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(x => x.Name)
                .ToList();
            Assert.IsTrue(fields.Count > 0);
            var countBytes = 0;
            foreach (var fieldInfo in fields)
            {
                var fieldTypeName = fieldInfo.FieldType.Name;
                switch (fieldTypeName)
                {
                    case "Boolean":
                        fieldInfo.SetValue(instance, reader.ReadBoolean());
                        countBytes += 1;
                        break;
                    case "Int32":
                        fieldInfo.SetValue(instance, reader.ReadInt32());
                        countBytes += 4;
                        break;
                    case "Single":
                        fieldInfo.SetValue(instance, reader.ReadSingle());
                        countBytes += 4;
                        break;
                    default:
                        throw new UnityException($"unsupported field type: {fieldTypeName}");
                }
            }
            return new Tuple<T, int>(instance, countBytes);
        }
    }
}