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

        public static DataCarrier Instance { get; private set; }
        public ServerClan clanToView;
        public PlayerData playerToView;

        private static Dictionary<string, object> s_datastorage = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            clanToView = null;
            playerToView = null;
        }

        public static void AddData<T>(string key, T value) where T : class
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

        public static T GetData<T>(string key, bool clear = true) where T : class
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
                Debug.LogWarning($"Cannot find Data: Data with supplied key ({key}) cannot be found in DataCarrier.");
                return null;
            }
        }
    }
}
