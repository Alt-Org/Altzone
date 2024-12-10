using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieChartManager : MonoBehaviour
{   
    // listataan palaset, montako niitä on
    [SerializeField] private List<Image> slices;

    // Hakee Unity:n UI:n tekstikentän josta tietoa halutaan hakea.
    [SerializeField] private TMP_Text impactForceText;
    [SerializeField] private TMP_Text healthPointsText;
    [SerializeField] private TMP_Text resistanceText;
    [SerializeField] private TMP_Text characterSizeText;
    [SerializeField] private TMP_Text speedText;

    // Asetetaan väri, minkälaiseksi palanen tulee muuttua tietyn statin mukaan. 
    // Näitä voidaan muuttaa suoraan unityn sisällä.
    [SerializeField] private Color impactForceColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color healthPointsColor = Color.green;
    [SerializeField] private Color resistanceColor = new Color(0.5f, 0f, 0.5f);
    [SerializeField] private Color characterSizeColor = Color.blue;
    [SerializeField] private Color speedColor = new Color(0f, 0.5f, 0f);
    [SerializeField] private Color defaultColor = Color.white;

    private void OnEnable()
    {
        // Päivitetään pie chart, kun paneeli/sivu avataan uudelleen
        Debug.Log("Pie Chart Manager: Paneeli avattu, päivitetään pie chart...");
        UpdateChart();
    }

    public void UpdateChart()
    {
        Debug.Log("Updating Pie Chart...");

        // Haetaan arvot tekstikentistä (TMP_Text) ja muutetaan ne numero (int) luvuiksi
        int impactForce = ParseText(impactForceText.text);
        int healthPoints = ParseText(healthPointsText.text);
        int resistance = ParseText(resistanceText.text);
        int characterSize = ParseText(characterSizeText.text);
        int speed = ParseText(speedText.text);

        Debug.Log($"Impact Force: {impactForce}, Health Points: {healthPoints}, Resistance: {resistance}, Character Size: {characterSize}, Speed: {speed}");

        // Järjestää statit
        var stats = new List<(int level, Color color)>
        {
            (impactForce, impactForceColor),
            (healthPoints, healthPointsColor),
            (resistance, resistanceColor),
            (characterSize, characterSizeColor),
            (speed, speedColor)
        };

        // Alustaa kaikki slicet
        foreach (var slice in slices)
        {
            slice.fillAmount = 1f / slices.Count;
            slice.color = defaultColor;
        }

        // Täytetään Palaset (slice) järjestyksessä PieChartiin.
        int currentSlice = 0;

        foreach (var stat in stats)
        {
            int level = stat.level;
            Color color = stat.color;

            for (int i = 0; i < level; i++)
            {
                if (currentSlice < slices.Count)
                {
                    Debug.Log($"Setting Slice {currentSlice} to Color {color}");
                    slices[currentSlice].color = color;
                    currentSlice++;
                }
            }
        }

        Debug.Log("Pie Chart updated!");
    }

    //Parsettaa tekstin, onko se mahdollista muuttaa int luvuksi.
    private int ParseText(string text)
    {
        return int.TryParse(text, out int result) ? result : 0;
    }
}
