using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieChartManager : MonoBehaviour
{
    // listataan palaset, montako niitä on.
    // list the slices, how many there are.
    [SerializeField] private List<Image> slices;

    // Hakee Unity:n UI:n tekstikentän josta tietoa halutaan hakea.
    // Retrieves the text field in Unity's UI from which information is to be retrieved.
    [SerializeField] private TMP_Text impactForceText;
    [SerializeField] private TMP_Text healthPointsText;
    [SerializeField] private TMP_Text resistanceText;
    [SerializeField] private TMP_Text characterSizeText;
    [SerializeField] private TMP_Text speedText;

    // Asetetaan väri, minkälaiseksi palanen tulee muuttua tietyn statin mukaan. Näitä voidaan muuttaa suoraan unityn sisällä.
    // Set the color, what kind of piece the piece should turn into according to a certain stat. These can be changed directly inside unity.
    [SerializeField] private Color impactForceColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color healthPointsColor = Color.green;
    [SerializeField] private Color resistanceColor = new Color(0.5f, 0f, 0.5f);
    [SerializeField] private Color characterSizeColor = Color.blue;
    [SerializeField] private Color speedColor = new Color(0f, 0.5f, 0f);
    [SerializeField] private Color defaultColor = Color.white;

    // Väliaikainen muuttuja arvoille.
    // Temporary variable for values.
    private int lastImpactForce;
    private int lastHealthPoints;
    private int lastResistance;
    private int lastCharacterSize;
    private int lastSpeed;

    private void OnEnable()
    {
        // Päivitetään PieChart, kun paneeli/sivu avataan uudelleen.
        // Updates PieChart when panel/page is opened.
        Debug.Log("Pie Chart Manager: Paneeli avattu, päivitetään pie chart...");
        UpdateChart();
    }

    private void Update()
    {
        // Tarkistaa, ovatko arvot muuttuneet.
        // Checks are values changed.
        int currentImpactForce = ParseText(impactForceText.text);
        int currentHealthPoints = ParseText(healthPointsText.text);
        int currentResistance = ParseText(resistanceText.text);
        int currentCharacterSize = ParseText(characterSizeText.text);
        int currentSpeed = ParseText(speedText.text);

        if (currentImpactForce != lastImpactForce ||
            currentHealthPoints != lastHealthPoints ||
            currentResistance != lastResistance ||
            currentCharacterSize != lastCharacterSize ||
            currentSpeed != lastSpeed)
        {
            // Päivittää viimeisimmät arvot.
            // Updates latest values.
            lastImpactForce = currentImpactForce;
            lastHealthPoints = currentHealthPoints;
            lastResistance = currentResistance;
            lastCharacterSize = currentCharacterSize;
            lastSpeed = currentSpeed;

            UpdateChart();
        }
    }



    public void UpdateChart()
    {
        Debug.Log("Updating Pie Chart...");

        // Haetaan arvot tekstikentistä (TMP_Text) ja muutetaan ne numero (int) luvuiksi.
        // Retrieve values ​​from text fields (TMP_Text) and change them to numbers (int).
        int impactForce = ParseText(impactForceText.text);
        int healthPoints = ParseText(healthPointsText.text);
        int resistance = ParseText(resistanceText.text);
        int characterSize = ParseText(characterSizeText.text);
        int speed = ParseText(speedText.text);

        Debug.Log($"Impact Force: {impactForce}, Health Points: {healthPoints}, Resistance: {resistance}, Character Size: {characterSize}, Speed: {speed}");

        // Järjestää statit.
        // Arrange stats.
        var stats = new List<(int level, Color color)>
        {
            (impactForce, impactForceColor),
            (healthPoints, healthPointsColor),
            (resistance, resistanceColor),
            (characterSize, characterSizeColor),
            (speed, speedColor)
        };

        // Alustaa kaikki slicet.
        // Formats all slices.
        foreach (var slice in slices)
        {
            slice.fillAmount = 1f / slices.Count;
            slice.color = defaultColor;
        }

        // Täytetään palaset (slice) järjestyksessä PieChartiin.
        // Fill up slices in order to the PieChart.
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

    // Parsettaa tekstin, onko se mahdollista muuttaa int luvuksi.
    // Parsing text are they possible to change from string -> int.
    private int ParseText(string text)
    {
        return int.TryParse(text, out int result) ? result : 0;
    }
}
