using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts
{
    [Flags] public enum What
    {
        None = 0,
        All = 1,
        Features = 2,
        Variables = 4,
    }

    /// <summary>
    /// Synchronize runtime game config over network using <c>byte</c> array.
    /// </summary>
    /// <remarks>
    /// Only Master Client can do this while in a room.
    /// </remarks>
    public class GameConfigSynchronizer : MonoBehaviour
    {
        private const int MsgSynchronize = PhotonEventDispatcher.EventCodeBase + 0;
        private const byte EndByte = 0xFE;
        private const int OverheadBytes = 2;

        private static readonly Dictionary<Type, int> TypesSizesCache = new Dictionary<Type, int>();

        /// <summary>
        /// Listens room Synchronize events from Photon Master Client.
        /// </summary>
        public static void Listen()
        {
            // Instantiate our private instance for listening synchronize events
            Get();
        }

        /// <summary>
        /// Sends room Synchronize events (Photon Master Client).
        /// </summary>
        public static void Synchronize(What what)
        {
            //--Debug.Log($"Synchronize {what}");
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can synchronize in a room");
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Features))
            {
                Get().SendSynchronizeFeatures((byte)What.Features, EndByte);
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Variables))
            {
                Get().SendSynchronizeVariables((byte)What.Variables, EndByte);
            }
        }

        private static GameConfigSynchronizer Get()
        {
            return _instance ??= UnitySingleton.CreateStaticSingleton<GameConfigSynchronizer>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }
        
        private static GameConfigSynchronizer _instance;

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            Debug.Log($"{name}");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgSynchronize, data => OnSynchronize(data.CustomData));
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
        }

        private static void OnSynchronize(object data)
        {
            if (data is byte[] bytes)
            {
                Assert.IsTrue(bytes.Length > 2);
                var lastByte = bytes[bytes.Length - 1];
                Assert.AreEqual(EndByte, lastByte, $"invalid synchronization message end: {lastByte}");
                var firstByte = bytes[0];
                Assert.IsTrue(firstByte > 0);
                if (firstByte == (byte)What.Features)
                {
                    ReadFeatures(bytes);
                }
                else if (firstByte == (byte)What.Variables)
                {
                    ReadVariables(bytes);
                }
                else
                {
                    throw new UnityException($"invalid synchronization message start: {firstByte}");
                }
            }
        }

        private void SendSynchronizeFeatures(byte first, byte last)
        {
            var features = Battle0GameConfig.Get().Features;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    BinarySerializer.Serialize(features, writer);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = features.GetType();
                var fieldsByteSize = CountFieldsByteSize(type);
                //--Debug.Log($"send data> {bytes.Length}: {string.Join(", ", bytes)}");
                Assert.AreEqual(fieldsByteSize, bytes.Length,
                    $"mismatch in type {type} fields size {fieldsByteSize} and written fields size {bytes.Length}");
                _photonEventDispatcher.RaiseEvent(MsgSynchronize, bytes);
            }
        }

        private static void ReadFeatures(byte[] bytes)
        {
            //--Debug.Log($"recv data< {bytes.Length}: {string.Join(", ", bytes)}");
            GameFeatures features;
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    features = BinarySerializer.Deserialize<GameFeatures>(reader).Item1;
                    reader.ReadByte(); // skip last
                }
            }
            Battle0GameConfig.Get().Features = features;
        }

        private void SendSynchronizeVariables(byte first, byte last)
        {
            var variables = Battle0GameConfig.Get().Variables;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    BinarySerializer.Serialize(variables, writer);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = variables.GetType();
                var fieldsByteSize = CountFieldsByteSize(type);
                //--Debug.Log($"send data> {bytes.Length}: {string.Join(", ", bytes)}");
                Assert.AreEqual(fieldsByteSize, bytes.Length,
                    $"mismatch in type {type} fields size {fieldsByteSize} and written fields size {bytes.Length}");
                _photonEventDispatcher.RaiseEvent(MsgSynchronize, bytes);
            }
        }

        private static void ReadVariables(byte[] bytes)
        {
            //--Debug.Log($"recv data< {bytes.Length}: {string.Join(", ", bytes)}");
            GameVariables variables;
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    variables = BinarySerializer.Deserialize<GameVariables>(reader).Item1;
                    reader.ReadByte(); // skip last
                }
            }
            Battle0GameConfig.Get().Variables = variables;
        }

        private static int CountFieldsByteSize(Type type)
        {
            if (TypesSizesCache.TryGetValue(type, out var size))
            {
                return size;
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var countBytes = OverheadBytes; // stream start and end bytes
            foreach (var fieldInfo in fields)
            {
                var fieldTypeName = fieldInfo.FieldType.Name;
                switch (fieldTypeName)
                {
                    case "Boolean":
                        countBytes += 1;
                        break;
                    case "Int32":
                        countBytes += 4;
                        break;
                    case "Single":
                        countBytes += 4;
                        break;
                    default:
                        throw new UnityException($"unknown field type: {fieldTypeName}");
                }
            }
            TypesSizesCache.Add(type, countBytes);
            return countBytes;
        }
    }
}