using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureGenerator : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI setText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI weightText;

    [Header("Rarity Visual")]
    public Image rarityImage;

    [Header("Rarity Colors")]
    public Color grayRarity = Color.gray;
    public Color blueRarity = Color.cyan;
    public Color yellowRarity = Color.yellow;

    private static readonly string[] names = { "Sofa", "Lamp", "Chair", "Fridge", "Door" };
    private static readonly string[] sets = { "Set 1", "Set 2", "Set 3" };


    void Start()
    {
        Generate();
    }


    public void Generate()
    {
        // Random Name and Set
        nameText.text = names[Random.Range(0, names.Length)];
        setText.text = sets[Random.Range(0, sets.Length)];

        // Random Value and Weight
        int value = Random.Range(1, 101);
        int weight = Random.Range(1, 101);
        valueText.text = $"Value: {value}";
        weightText.text = $"Weight: {weight}kg";

        // Random Rarity Color
        int rarityRoll = Random.Range(0, 3);
        switch (rarityRoll)
        {
            case 0:
                rarityImage.color = grayRarity;
                break;
            case 1:
                rarityImage.color = blueRarity;
                break;
            case 2:
                rarityImage.color = yellowRarity;
                break;
        }
    }
}
