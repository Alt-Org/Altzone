using System;
using System.Collections.Generic;
using UnityEngine;

public class CarbonFootprint : MonoBehaviour
{
    private float lastCheckTime = 0f;

    private float sessionTime = 0f; // Session kesto sekunneissa
    private float powerConsumption = 2f; // Oletusvirrankulutus watteina
    private float co2PerKwh = 475f; // CO2 per Kwh (grammoina)
    private float batteryCapacity = 3000f; // Oletus akkukapasiteetti mAh

    private float dataUsage = 0f; // Datankäyttö MB
    private float co2PerMB = 0.02f; // CO2 per megatavu

    [SerializeField] private float dataUsedPerSecond = 0.5f; // MB per sekunti
    static float carbonCount; // Hiilijalanjäljen kokonaismäärä grammoina

    public static float CarbonCount
    {
        get => carbonCount;
    }

    [Header("Carbon Tracking")]
    [SerializeField] private float baselineConsumption = 2f; // Peruskulutus watteina
    [SerializeField] private float networkMultiplier = 1.5f; // Verkkoliikenteen kerroin

    private List<float> carbonHistory = new List<float>();
    private DateTime lastSaveTime;

    public static CarbonFootprint Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            powerConsumption = GetDevicePowerConsumption();
            batteryCapacity = GetBatteryCapacity();
        }
    }

    private void Update()
    {
        UpdateCarbon();
    }

    /// <summary>
    /// Laskee ja päivittää sovelluksen käytöstä aiheutuvan hiilijalanjäljen grammoina CO2.
    /// </summary>
    private void UpdateCarbon()
    {
        sessionTime += Time.deltaTime;

        // Lasketaan peruskulutus kilowattitunteina ja muunnetaan CO2-päästöiksi
        float energyConsumed = (sessionTime / 3600f) * (baselineConsumption / 1000f); // kWh
        carbonCount = energyConsumed * co2PerKwh;

        // Lasketaan verkkoyhteyden vaikutus hiilijalanjälkeen
        dataUsage += dataUsedPerSecond * Time.deltaTime * networkMultiplier;
        carbonCount += dataUsage * co2PerMB;

        SaveCarbonHistory();

        // Tulostetaan tuloksia kerran sekunnissa
        if (sessionTime - lastCheckTime >= 1f)
        {
            Debug.Log($"CO2 per tunti: {carbonCount / (sessionTime / 3600f)} g CO2");
            lastCheckTime = sessionTime;
        }
    }

    private void SaveCarbonHistory()
    {
        // Tallennetaan hiilijalanjälki minuutin välein
        if (DateTime.Now - lastSaveTime >= TimeSpan.FromMinutes(1))
        {
            carbonHistory.Add(carbonCount);
            lastSaveTime = DateTime.Now;
        }
    }

    public void ResetCarbon()
    {
        carbonCount = 0;
        carbonHistory.Clear();
        sessionTime = 0;
        dataUsage = 0;
        lastSaveTime = DateTime.Now;
    }

    private float GetDevicePowerConsumption()
    {
        float powerUsage = 2f;

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using (AndroidJavaObject intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED"))
                    {
                        using (AndroidJavaObject batteryIntent = currentActivity.Call<AndroidJavaObject>("registerReceiver", null, intentFilter))
                        {
                            int level = batteryIntent.Call<int>("getIntExtra", "level", -1);
                            int scale = batteryIntent.Call<int>("getIntExtra", "scale", -1);
                            int batteryCapacity = 3000;

                            float batteryLevel = (level / (float)scale) * batteryCapacity;
                            powerUsage = batteryLevel / 1000f;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Virrankulutustietojen haku epäonnistui: " + e.Message);
            }
        }

        return powerUsage;
    }

    private float GetBatteryCapacity()
    {
        float capacity = 3000f;

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass batteryManager = new AndroidJavaClass("android.os.BatteryManager"))
                using (AndroidJavaClass powerManager = new AndroidJavaClass("android.os.PowerManager"))
                {
                    var batteryCapField = batteryManager.GetStatic<int>("BATTERY_PROPERTY_CAPACITY");
                    if (batteryCapField > 0)
                    {
                        capacity = batteryCapField;
                    }

                    var powerSaveMode = powerManager.Call<bool>("isPowerSaveMode");
                    if (powerSaveMode)
                    {
                        capacity *= 0.7f;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Akkukapasiteetin haku epäonnistui: {e.Message}");
            }
        }
        return capacity;
    }
}
