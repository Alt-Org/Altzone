using UnityEngine;
using System;
using System.Linq;
using System.Text;
using Photon.Client;

namespace Altzone.PhotonSerializer
{
    /// <summary>
    /// This file is copied and somewhat modified from: https://github.com/MS-LIMA/PhotonSerializer?tab=readme-ov-file
    /// </summary>
    public class Serializer
    {
        /*public static void RegisterCustomType<T>(byte code)
        {
            var type = typeof(T);

            var methodInfos = type.GetMethods();

            Func<object, byte[]> serializeMethod = null;
            Func<byte[], object> deserializeMethod = null;

            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name == "Serialize")
                {
                    if (!methodInfo.IsStatic)
                    {
                        Debug.LogError(
                            $"Serialize method must be static! Registering custom type \"{type.ToString()}\" failed.");
                        return;
                    }

                    serializeMethod = (Func<object, byte[]>)methodInfo.CreateDelegate(typeof(Func<object, byte[]>));
                }
                if (methodInfo.Name == "Deserialize")
                {
                    if (!methodInfo.IsStatic)
                    {
                        Debug.LogError(
                            $"Deserialize method must be static! Registering custom type \"{type.ToString()}\" failed.");
                        return;
                    }

                    deserializeMethod = (Func<byte[], object>)methodInfo.CreateDelegate(typeof(Func<byte[], object>));
                }
            };

            if (serializeMethod == null)
            {
                Debug.LogError($"There is no serialize method! Registering custom type \"{type.ToString()}\" failed.");
                return;
            }

            if (deserializeMethod == null)
            {
                Debug.LogError($"There is no serialize method! Registering custom type \"{type.ToString()}\" failed.");
                return;
            }

            PhotonPeer.RegisterType(
                type,
                code,
                x => serializeMethod(x),
                x => deserializeMethod(x)
           );
        }*/

        public static byte[] JoinBytes(params byte[][] bytes)
        {
            var rv = new byte[bytes.Sum(x => x.Length)];
            var offset = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i].CopyTo(rv, offset);
                offset += bytes[i].Length;
            }

            return rv;
        }

        public static void JoinBytes(ref byte[] bytesArray, byte[] joinBytes)
        {
            bytesArray = bytesArray.Concat(joinBytes).ToArray();
        }

        public static void JoinBytes(ref byte[] bytesArray, params byte[][] joinBytes)
        {
            for (var i = 0; i < joinBytes.Length; i++)
            {
                bytesArray = bytesArray.Concat(joinBytes[i]).ToArray();
            }
        }

        #region Serialize
        public static void Serialize(int value, ref byte[] bytes)
        {
            var _bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(long value, ref byte[] bytes)
        {
            var _bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(float value, ref byte[] bytes)
        {
            var _bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(double value, ref byte[] bytes)
        {
            var _bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(byte[] value, ref byte[] bytes)
        {
            var length = value?.Length ?? 0;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                JoinBytes(ref bytes, value);
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(byte value, ref byte[] bytes)
        {
            var _bytes = new byte[] { value };
            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(bool value, ref byte[] bytes)
        {
            var _bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            JoinBytes(ref bytes, _bytes);
        }

        public static void Serialize(Vector3 value, ref byte[] bytes)
        {
            var x = BitConverter.GetBytes(value.x);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);

            var y = BitConverter.GetBytes(value.y);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(y);

            var z = BitConverter.GetBytes(value.z);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(z);

            JoinBytes(ref bytes, x, y, z);
        }

        public static void Serialize(Vector2 value, ref byte[] bytes)
        {
            var x = BitConverter.GetBytes(value.x);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);

            var y = BitConverter.GetBytes(value.y);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(y);

            JoinBytes(ref bytes, x, y);
        }

        public static void Serialize(Quaternion value, ref byte[] bytes)
        {
            var x = BitConverter.GetBytes(value.x);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);

            var y = BitConverter.GetBytes(value.y);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(y);

            var z = BitConverter.GetBytes(value.z);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(z);

            var w = BitConverter.GetBytes(value.w);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(w);

            JoinBytes(ref bytes, x, y, z, w);
        }

        public static void Serialize(string value, ref byte[] bytes)
        {
            if (string.IsNullOrEmpty(value))
            {
                Serialize(0, ref bytes);
                return;
            }

            var stringBytes = Encoding.UTF8.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(stringBytes);

            var lengthBytes = BitConverter.GetBytes(stringBytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            JoinBytes(ref bytes, lengthBytes, stringBytes);
        }

        public static void Serialize(int[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(float[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(bool[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(Vector2[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(Vector3[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(Quaternion[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }

        public static void Serialize(string[] values, ref byte[] bytes)
        {
            var length = values == null ? 0 : values.Length;
            if (length > 0)
            {
                Serialize(length, ref bytes);
                for (var i = 0; i < values.Length; i++)
                {
                    Serialize(values[i], ref bytes);
                }
            }
            else
            {
                Serialize(0, ref bytes);
            }
        }
        #endregion

        #region Deserialize
        public static int DeserializeInt(byte[] bytes, ref int offset)
        {
            var _bytes = new byte[4];
            Array.Copy(bytes, offset, _bytes, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            offset += 4;
            return BitConverter.ToInt32(_bytes, 0);

            // int, float, bool vecot3, ve2 ,qua, string
        }

        public static byte DeserializeByte(byte[] bytes, ref int offset)
        {
            var _byte = bytes[offset];

            offset += 1;
            return _byte;
        }

        public static long DeserializeLong(byte[] bytes, ref int offset)
        {
            var _bytes = new byte[8];
            Array.Copy(bytes, offset, _bytes, 0, 8);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            offset += 8;
            return BitConverter.ToInt64(_bytes, 0);

            // int, float, bool vecot3, ve2 ,qua, string
        }

        public static byte[] DeserializeByteArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new byte[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeByte(bytes, ref offset);
                }

                return array;
            }

            return new byte[0];
        }

        public static float DeserializeFloat(byte[] bytes, ref int offset)
        {
            var _bytes = new byte[4];
            Array.Copy(bytes, offset, _bytes, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            offset += 4;
            return BitConverter.ToSingle(_bytes, 0);
        }
        public static double DeserializeDouble(byte[] bytes, ref int offset)
        {
            var _bytes = new byte[8];
            Array.Copy(bytes, offset, _bytes, 0, 8);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            offset += 8;
            return BitConverter.ToDouble(_bytes, 0);
        }

        public static bool DeserializeBool(byte[] bytes, ref int offset)
        {
            var _bytes = new byte[1];
            Array.Copy(bytes, offset, _bytes, 0, 1);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);

            offset += 1;
            return BitConverter.ToBoolean(_bytes, 0);
        }

        public static Vector3 DeserializeVector3(byte[] bytes, ref int offset)
        {
            var _xBytes = new byte[4];
            var _yBytes = new byte[4];
            var _zBytes = new byte[4];

            Array.Copy(bytes, offset, _xBytes, 0, 4);
            Array.Copy(bytes, offset + 4, _yBytes, 0, 4);
            Array.Copy(bytes, offset + 8, _zBytes, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_xBytes);
                Array.Reverse(_yBytes);
                Array.Reverse(_zBytes);
            }

            var o = new Vector3
            {
                x = BitConverter.ToSingle(_xBytes, 0),
                y = BitConverter.ToSingle(_yBytes, 0),
                z = BitConverter.ToSingle(_zBytes, 0)
            };

            offset += 12;
            return o;
        }

        public static Vector2 DeserializeVector2(byte[] bytes, ref int offset)
        {
            var _xBytes = new byte[4];
            var _yBytes = new byte[4];

            Array.Copy(bytes, offset, _xBytes, 0, 4);
            Array.Copy(bytes, offset + 4, _yBytes, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_xBytes);
                Array.Reverse(_yBytes);
            }

            var o = new Vector2
            {
                x = BitConverter.ToSingle(_xBytes, 0),
                y = BitConverter.ToSingle(_yBytes, 0)
            };

            offset += 8;
            return o;
        }

        public static Quaternion DeserializeQuaternion(byte[] bytes, ref int offset)
        {
            var _xBytes = new byte[4];
            var _yBytes = new byte[4];
            var _zBytes = new byte[4];
            var _wBytes = new byte[4];

            Array.Copy(bytes, offset, _xBytes, 0, 4);
            Array.Copy(bytes, offset + 4, _yBytes, 0, 4);
            Array.Copy(bytes, offset + 8, _zBytes, 0, 4);
            Array.Copy(bytes, offset + 12, _wBytes, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_xBytes);
                Array.Reverse(_yBytes);
                Array.Reverse(_zBytes);
                Array.Reverse(_wBytes);
            }

            var o = new Quaternion
            {
                x = BitConverter.ToSingle(_xBytes, 0),
                y = BitConverter.ToSingle(_yBytes, 0),
                z = BitConverter.ToSingle(_zBytes, 0),
                w = BitConverter.ToSingle(_wBytes, 0)
            };

            offset += 16;
            return o;
        }

        public static string DeserializeString(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var _bytes = new byte[length];
                Array.Copy(bytes, offset, _bytes, 0, length);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(_bytes);

                offset += length;
                return Encoding.UTF8.GetString(_bytes);
            }

            return "";
        }

        public static int[] DeserializeIntArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new int[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeInt(bytes, ref offset);
                }

                return array;
            }

            return new int[0];
        }

        public static float[] DeserializeFloatArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new float[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeFloat(bytes, ref offset);
                }

                return array;
            }

            return new float[0];
        }

        public static bool[] DeserializeBoolArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new bool[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeBool(bytes, ref offset);
                }

                return array;
            }

            return new bool[0];
        }

        public static Vector3[] DeserializeVector3Array(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new Vector3[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeVector3(bytes, ref offset);
                }

                return array;
            }

            return new Vector3[0];
        }

        public static Vector2[] DeserializeVector2Array(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new Vector2[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeVector2(bytes, ref offset);
                }

                return array;
            }

            return new Vector2[0];
        }

        public static Quaternion[] DeserializeQuaternionArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new Quaternion[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeQuaternion(bytes, ref offset);
                }

                return array;
            }

            return new Quaternion[0];
        }

        public static string[] DeserializeStringArray(byte[] bytes, ref int offset)
        {
            var length = DeserializeInt(bytes, ref offset);
            if (length > 0)
            {
                var array = new string[length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = DeserializeString(bytes, ref offset);
                }

                return array;
            }

            return new string[0];
        }
        #endregion
    }
}
