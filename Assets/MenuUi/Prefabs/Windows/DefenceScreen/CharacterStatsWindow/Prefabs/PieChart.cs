using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieChart : MonoBehaviour
{
    [Header("Settings")]
    public int totalSlices = 30; // Yhteensä 30 osaa
    public Color[] statColors; // Värit: Iskuvoima, elinvoima jne.
    public int[] baseStats; // Perusstatit (esim. [5, 3, 3, 4, 3, 4])
    public int[] playerChoices; // Pelaajan valinnat

    [Header("References")]
    public GameObject slicePrefab; // Prefab yksittäiselle "osalle"
    public Transform pieChartParent; // Parent PieChartille

    private List<Image> slices = new List<Image>(); // Tallennetaan kaikki osat

    void Start()
    {

    }
    void OnEnable()
    {
        Debug.Log("OnEnable method is called.");
        GeneratePieChart();
        UpdatePieChart();
    }

    void GeneratePieChart()
    {
        Debug.Log("GeneratePieChart method is called.");

        foreach (Transform child in pieChartParent)
        {
            Destroy(child.gameObject);
        }
        slices.Clear();

        float rotationStep = 360f / totalSlices;
        for (int i = 0; i < totalSlices; i++)
        {
            GameObject sliceObj = Instantiate(slicePrefab, pieChartParent);
            if (sliceObj == null)
            {
                Debug.LogError($"Slice prefab instantiation failed at index {i}");
                continue;
            }

            Debug.Log($"Slice {i} instantiated.");

            Image sliceImage = sliceObj.GetComponent<Image>();
            if (sliceImage == null)
            {
                Debug.LogError($"Slice prefab is missing an Image component at index {i}");
                continue;
            }

            sliceImage.fillAmount = 1f / totalSlices;
            sliceObj.transform.localRotation = Quaternion.Euler(0, 0, -rotationStep * i);
            slices.Add(sliceImage);

            Debug.Log($"Slice {i} fillAmount set to {sliceImage.fillAmount} and rotation to {-rotationStep * i}");
        }

        Debug.Log("All slices have been generated.");
    }



    void UpdatePieChart()
    {
        int sliceIndex = 0;

        // Käy läpi jokainen stat
        for (int statIndex = 0; statIndex < baseStats.Length; statIndex++)
        {
            int totalStatSlices = baseStats[statIndex] + playerChoices[statIndex];

            Debug.Log($"StatIndex: {statIndex}, TotalStatSlices: {totalStatSlices}");

            for (int j = 0; j < totalStatSlices; j++)
            {
                if (sliceIndex < slices.Count)
                {
                    slices[sliceIndex].color = statColors[statIndex]; // Asetetaan oikea väri
                    Debug.Log($"Slice {sliceIndex} assigned color for stat {statIndex}");
                    sliceIndex++;
                }
            }
        }

        // Täytetään loput valkoisilla osilla
        for (int i = sliceIndex; i < slices.Count; i++)
        {
            slices[i].color = Color.white; // Valkoiset osat
            Debug.Log($"Slice {i} color set to white");
        }
    }


    public void AddStatPoint(int statIndex)
    {
        // Lisää yksi piste pelaajan valitsemalle statille
        if (playerChoices[statIndex] < totalSlices)
        {
            playerChoices[statIndex]++;
            UpdatePieChart();
        }
    }
}
