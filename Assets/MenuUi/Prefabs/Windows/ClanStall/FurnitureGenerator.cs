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

    [SerializeField] private int value;
    [SerializeField] private int weight;

    public int Value => value;
    

    void Start()
    {
        Generate();
    }

    //Generates the randomized rarity, value, weight, set and name for testing. Will be updated later to do proper values
    public void Generate()
    {
        nameText.text = names[Random.Range(0, names.Length)];
        setText.text = sets[Random.Range(0, sets.Length)];

        value = Random.Range(1, 101);
        weight = Random.Range(1, 101);

        UpdateDisplay();

        int rarityRoll = Random.Range(0, 3);
        switch (rarityRoll)
        {
            case 0: rarityImage.color = grayRarity; break;
            case 1: rarityImage.color = blueRarity; break;
            case 2: rarityImage.color = yellowRarity; break;
        }
    }

    public void SetValue(int newValue)
    {
        value = newValue;
        valueText.text = $"{value}gp";
    }

    public void SetWeight(int newWeight)
    {
        weight = newWeight;
        weightText.text = $"{weight}kg";
    }

    private void UpdateDisplay()
    {
        valueText.text = $"{value}gp";
        weightText.text = $"{weight}kg";
    }
}
