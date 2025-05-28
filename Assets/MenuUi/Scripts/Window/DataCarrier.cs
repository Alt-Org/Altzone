using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.Window
{
    public class DataCarrier : MonoBehaviour
    {
        public const string ClanListing = "cl";
        public const string PlayerProfile = "pp";
        public const string BattleUiEditorRequested = "bui";
        public const string RequestedWindow = "rw";


        public static DataCarrier Instance { get; private set; }
        public ServerClan clanToView;
        public PlayerData playerToView;

        private static Dictionary<string, object> s_datastorage = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            clanToView = null;
            playerToView = null;
        }

        public static void AddData<T>(string key, T value)
        {
            if (typeof(T).Equals(typeof(int)))
            {
                AddPrimitiveData(key, Convert.ToInt32(value));
            }
            else if (typeof(T).Equals(typeof(bool)))
            {
                AddPrimitiveData(key, Convert.ToBoolean(value));
            }
            else if (typeof(T).Equals(typeof(string)))
            {
                AddPrimitiveData(key, Convert.ToString(value));
            }
            else if (typeof(T).Equals(typeof(float)))
            {
                AddPrimitiveData(key, Convert.ToSingle(value));
            }
            else if (typeof(T).IsClass)
            {
                AddClassData<object>(key, value);
            }
        }

        public static void AddClassData<T>(string key, T value) where T : class
        {
            if (Instance == null)
            {
                GameObject carrier = Instantiate(new GameObject());
                carrier.AddComponent<DataCarrier>();
            }
            if (s_datastorage.ContainsKey(key))
            {
                Debug.LogWarning($"Cannot add Data: Data with supplied key ({key}) already exist in DataCarrier.");
                return;
            }
            s_datastorage.Add(key, value);
        }

        public static void AddPrimitiveData<T>(string key, T value) where T : IComparable<T>
        {
            if (Instance == null)
            {
                GameObject carrier = Instantiate(new GameObject());
                carrier.AddComponent<DataCarrier>();
            }
            if (s_datastorage.ContainsKey(key))
            {
                Debug.LogWarning($"Cannot add Data: Data with supplied key ({key}) already exist in DataCarrier.");
                return;
            }
            s_datastorage.Add(key, value);
        }

        public static T GetData<T>(string key, bool clear = true, bool suppressWarning = false)
        {
            if(typeof(T).Equals(typeof(int)))
            {
                return (T)(object)GetPrimitiveData<int>(key, clear, suppressWarning);
            }
            if (typeof(T).Equals(typeof(int?)))
            {
                int? value = GetNullablePrimitiveData<int>(key, clear, suppressWarning);
                if (value == null) return default;
                return (T)(object)value;
            }
            else if(typeof(T).Equals(typeof(bool)))
            {
                return (T)(object)GetPrimitiveData<bool>(key, clear, suppressWarning);
            }
            if (typeof(T).Equals(typeof(bool?)))
            {
                bool? value = GetNullablePrimitiveData<bool>(key, clear, suppressWarning);
                if (value == null) return default;
                return (T)(object)value;
            }
            else if(typeof(T).Equals(typeof(string)))
            {
                return (T)(object)GetPrimitiveData<string>(key, clear, suppressWarning);
            }
            else if(typeof(T).Equals(typeof(float)))
            {
                return (T)(object)GetPrimitiveData<float>(key, clear, suppressWarning);
            }
            else if (typeof(T).IsClass)
            {
                return (T)GetClassData<object>(key, clear, suppressWarning);
            }
            return default;
        }

        public static T GetClassData<T>(string key, bool clear = true, bool suppressWarning = false) where T : class
        {
            if (s_datastorage.ContainsKey(key))
            {
                T value = s_datastorage[key] as T;
                if (clear)
                {
                    s_datastorage.Remove(key);
                }
                return value;
            }
            else
            {
                if (!suppressWarning) Debug.LogWarning($"Cannot find Data: Data with supplied key ({key}) cannot be found in DataCarrier.");
                return null;
            }
        }

        public static T GetPrimitiveData<T>(string key, bool clear = true, bool suppressWarning = false) where T : IComparable
        {
            if (s_datastorage.ContainsKey(key))
            {
                T value = (T)s_datastorage[key];
                if (clear)
                {
                    s_datastorage.Remove(key);
                }
                return value;
            }
            else
            {
                if (!suppressWarning) Debug.LogWarning($"Cannot find Data: Data with supplied key ({key}) cannot be found in DataCarrier.");
                return default;
            }
        }

        public static T? GetNullablePrimitiveData<T>(string key, bool clear = true, bool suppressWarning = false) where T : struct
        {
            if (s_datastorage.ContainsKey(key))
            {
                T value = (T)s_datastorage[key];
                if (clear)
                {
                    s_datastorage.Remove(key);
                }
                return value;
            }
            else
            {
                if(!suppressWarning) Debug.LogWarning($"Cannot find Data: Data with supplied key ({key}) cannot be found in DataCarrier.");
                return null;
            }
        }
    }
}
