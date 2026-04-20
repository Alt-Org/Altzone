using System;
using UnityEngine;

public class CarbonFootprint : MonoBehaviour
{
    public static CarbonFootprint Instance { get; private set; }

    [Header("Carbon calculation defaults")]
    [SerializeField] private float _serverEnergyPerHour = 0.2f;      // kWh / h
    [SerializeField] private float _serverCarbonIntensity = 0.1f;    // kg CO2 / kWh

    [SerializeField] private float _deviceEnergyPerHour = 0.1f;      // kWh / h
    [SerializeField] private float _deviceCarbonIntensity = 0.1f;    // kg CO2 / kWh

    [SerializeField] private float _dataMbPerHour = 80f;             // MB / h
    [SerializeField] private float _dataCarbonPerGb = 0.02f;         // kg CO2 / GB

    private const string CarbonMonthKey = "CarbonFootprint_Month";
    private const string CarbonValueKey = "CarbonFootprint_Value";
    private const string CarbonLifetimeKey = "CarbonFootprint_Lifetime";

    private static float _carbonCount; // current month total, grams
    private static float _lifetimeCarbonCount; // optional total, grams

    public static float CarbonCount => _carbonCount;
    public static float LifetimeCarbonCount => _lifetimeCarbonCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSavedCarbon();
            CheckMonthReset();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        CheckMonthReset();
        UpdateCarbon(Time.deltaTime);
    }

    /// <summary>
    /// Updates monthly carbon footprint using default formula values.
    /// Result is stored in grams.
    /// </summary>
    private void UpdateCarbon(float deltaTime)
    {
        float hoursPlayed = deltaTime / 3600f;

        // Server consumption: P = t × E × I
        float serverKg = hoursPlayed * _serverEnergyPerHour * _serverCarbonIntensity;

        // Device consumption: L = t × D × J
        float deviceKg = hoursPlayed * _deviceEnergyPerHour * _deviceCarbonIntensity;

        // Data consumption:
        // convert MB/h -> GB for this frame, then multiply by carbon factor
        float dataGbThisFrame = (_dataMbPerHour * hoursPlayed) / 1000f;
        float dataKg = dataGbThisFrame * _dataCarbonPerGb;

        // Total in kg, then convert to grams
        float totalKg = serverKg + deviceKg + dataKg;
        float totalGrams = totalKg * 1000f;

        _carbonCount += totalGrams;
        _lifetimeCarbonCount += totalGrams;

        SaveCarbon();
    }

    /// <summary>
    /// Resets monthly value when month changes.
    /// </summary>
    private void CheckMonthReset()
    {
        string currentMonth = GetCurrentMonthKey();

        if (PlayerPrefs.GetString(CarbonMonthKey, string.Empty) != currentMonth)
        {
            _carbonCount = 0f;
            PlayerPrefs.SetString(CarbonMonthKey, currentMonth);
            PlayerPrefs.SetFloat(CarbonValueKey, _carbonCount);
            PlayerPrefs.Save();
        }
    }

    private void LoadSavedCarbon()
    {
        string savedMonth = PlayerPrefs.GetString(CarbonMonthKey, string.Empty);
        string currentMonth = GetCurrentMonthKey();

        if (savedMonth == currentMonth)
            _carbonCount = PlayerPrefs.GetFloat(CarbonValueKey, 0f);
        else
            _carbonCount = 0f;

        _lifetimeCarbonCount = PlayerPrefs.GetFloat(CarbonLifetimeKey, 0f);

        PlayerPrefs.SetString(CarbonMonthKey, currentMonth);
        PlayerPrefs.Save();
    }

    private void SaveCarbon()
    {
        PlayerPrefs.SetString(CarbonMonthKey, GetCurrentMonthKey());
        PlayerPrefs.SetFloat(CarbonValueKey, _carbonCount);
        PlayerPrefs.SetFloat(CarbonLifetimeKey, _lifetimeCarbonCount);
        PlayerPrefs.Save();
    }

    private string GetCurrentMonthKey()
    {
        DateTime now = DateTime.Now;
        return $"{now.Year:D4}-{now.Month:D2}";
    }

    public void ResetCurrentMonthCarbon()
    {
        _carbonCount = 0f;
        SaveCarbon();
    }

    public void ResetAllCarbon()
    {
        _carbonCount = 0f;
        _lifetimeCarbonCount = 0f;
        SaveCarbon();
    }
}

/*public class CarbonFootprint : MonoBehaviour
{
    private float lastCheckTime = 0f;
    private float sessionTime = 0f;

    private float carbonTargetPerMinute = 55f; // CO2 per minuutti
    private float carbonTargetPerSecond;

    static float carbonCount;

    public static float CarbonCount
    {
        get => carbonCount;
    }

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
        // Jakaa CO2/min 60f:llä
        carbonTargetPerSecond = carbonTargetPerMinute / 60f;
    }

    private void Update()
    {
        UpdateCarbon();
    }

    /// <summary>
    /// Laskee ja päivittää hiilijalanjäljen per sekunti
    /// </summary>
    private void UpdateCarbon()
    {
        sessionTime += Time.deltaTime;

        // Hiilijalanjälki nousee ajanmyötä
        carbonCount += carbonTargetPerSecond * Time.deltaTime;

        SaveCarbonHistory();

        // Tulostaa konsoliin CO2 määrän ja per minuutti
        if (sessionTime - lastCheckTime >= 1f)
        {
            //Debug.Log($"Current total: {carbonCount} g CO2");
            lastCheckTime = sessionTime;
        }
    }


    // Tämän kuuluisi tallentaa sen minuutin välein. Ei toimi as of now.
    void SaveCarbonHistory()
    {
        // Päivittää minuutin välein hiilijalanjäljen.
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
        lastSaveTime = DateTime.Now;
    }
}*/
