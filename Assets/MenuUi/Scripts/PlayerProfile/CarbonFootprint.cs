using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class CarbonFootprint : MonoBehaviour
{
    private float lastCheckTime = 0f;

    // Hiilijalanjälki jutut!
    private float sessionTime = 0f; // laskee sessioajan
    private float powerConsumption = 2f; // Oletusvirrankulutus per sekunti
    private float co2PerKwh = 475f; // co2 per Kwh
    private float batteryCapacity = 3000f; // Oletus akkukapasiteetti mAh (tarkennetaan jos saatavilla)

    private float dataUsage = 0f; // datankäyttö
    private float co2PerMB = 0.02f; // co2 per megatavu

    [SerializeField] private float dataUsedPerSecond = 0.5f; // Esimerkiksi MB per sekunti

    private int minuteCount;
    private float secondsCount;
    private float countToCarbon;
    static float carbonCount;

    public static float CarbonCount
    {
        get => carbonCount;
    }

    private ServerPlayer _player;

    [Header("Carbon Tracking")]
    [SerializeField] private float baselineConsumption = 2f; // Peruskulutus watteina
    [SerializeField] private float networkMultiplier = 1.5f; // Verkkoliikenteen kerroin
    [SerializeField] private bool enableDetailedTracking = true; // Tarkemman seurannan kytkentä

    // Lista hiilijalanjäljen historiatiedoille
    private List<float> carbonHistory = new List<float>();
    private DateTime lastSaveTime;

    public static CarbonFootprint Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else
        {
            Destroy(gameObject); // Enforce singleton
        }
    }


    private void Start()
    {
        // Yritetään hakea virrankulutustietoja Android-laitteella
        if (Application.platform == RuntimePlatform.Android)
        {
            powerConsumption = GetDevicePowerConsumption();
            batteryCapacity = GetBatteryCapacity();
        }
    }


    private void Update()
    {
        updateCarbon();
    }


    /// <summary>
    /// Laskee ja päivittää sovelluksen käytöstä aiheutuvan hiilijalanjäljen.
    /// Huomioi sekä laitteen virrankulutuksen että verkkoyhteyden datankäytön.
    /// </summary>
    /// <remarks>
    /// Laskenta perustuu kahteen päätekijään:
    /// 1. Virrankulutus: 
    ///    - Laskee kulutetun energian (kWh) session keston ja tehonkulutuksen perusteella
    ///    - Muuntaa energian CO2-päästöiksi kertoimella co2PerKwh
    /// 2. Datankäyttö:
    ///    - Seuraa kertynyttä datankäyttöä (MB)
    ///    - Muuntaa datan CO2-päästöiksi kertoimella co2PerMB
    /// 
    /// Päivittää tuloksen käyttöliittymään grammoina CO2.
    /// </remarks>
    private void updateCarbon()
    {
        sessionTime += Time.deltaTime;

        // Lasketaan peruskulutus käyttäen määritettyä peruskulutusta
        float energyConsumed = sessionTime / 3600f * baselineConsumption;
        carbonCount = energyConsumed * co2PerKwh;

        // Lasketaan verkkoyhteyden vaikutus käyttäen kerrointa
        dataUsage += dataUsedPerSecond * Time.deltaTime * networkMultiplier;
        carbonCount += dataUsage * co2PerMB;

        SaveCarbonHistory();

        if (lastCheckTime + 10f < sessionTime)
        {  
            Debug.Log($"powerConsumption: {powerConsumption}, batteryCapacity: {batteryCapacity}, dataUsage: {dataUsage}");
            lastCheckTime = sessionTime;
        }
    }

    private void SaveCarbonHistory()
    {
        // Tallennetaan hiilijalanjälki minuutin välein
        if (DateTime.Now - lastSaveTime >= TimeSpan.FromSeconds(1))
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


    // <summary>
    // Hakee laitteen virrankulutustiedot Android-järjestelmästä.
    // </summary>
    // <returns>Virrankulutus kilowattitunteina (kWh)</returns>
    // <remarks>
    // Toimintaperiaate:
    // 1. Tarkistaa ensin onko kyseessä Android-laite
    // 2. Android-laitteella:
    //    - Hakee akun varaustason (%) käyttöjärjestelmän BATTERY_CHANGED-kuuntelijan avulla
    //    - Laskee virrankulutuksen akun kapasiteetin (3000mAh) ja varaustason perusteella
    // 3. Muilla alustoilla:
    //    - Käyttää oletusarvoa 2.0 kWh
    //
    // Virheenkäsittely:
    // - Jos Android-tietojen haku epäonnistuu, palauttaa oletusarvon ja kirjaa varoituksen
    // 
    // Huom: Akkukapasiteetti (3000mAh) on kiinteä oletusarvo, joka voi vaihdella laitekohtaisesti
    // </remarks>
    private float GetDevicePowerConsumption()
    {
        float powerUsage = 2f; // Oletusarvoinen virrankulutus per sekunti

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
                            int batteryCapacity = 3000; // Oletusarvo akkukapasiteetti mAh

                            float batteryLevel = (level / (float)scale) * batteryCapacity;
                            powerUsage = batteryLevel / 1000f; // Muutetaan kWh:ksi
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Virrankulutustietojen haku epäonnistui: " + e.Message);
            }

        }

        return powerUsage;
    }

    // Funktio, joka hakee laitteen akkukapasiteetin
    private float GetBatteryCapacity()
    {
        float capacity = 3000f; // Oletusarvo

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass batteryManager = new AndroidJavaClass("android.os.BatteryManager"))
                using (AndroidJavaClass powerManager = new AndroidJavaClass("android.os.PowerManager"))
                {
                    // Yritetään hakea todellinen akkukapasiteetti
                    var batteryCapField = batteryManager.GetStatic<int>("BATTERY_PROPERTY_CAPACITY");
                    if (batteryCapField > 0)
                    {
                        capacity = batteryCapField;
                    }

                    // Haetaan myös virransäästötila
                    var powerSaveMode = powerManager.Call<bool>("isPowerSaveMode");
                    if (powerSaveMode)
                    {
                        // Virransäästötilassa kulutus on pienempi
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
