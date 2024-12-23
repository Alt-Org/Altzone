using System;
using System.Collections.Generic;
using UnityEngine;

public class CarbonFootprint : MonoBehaviour
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
}
