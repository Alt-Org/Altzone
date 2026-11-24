using UnityEngine;
using UnityEngine.UI;

public class RandomBG : MonoBehaviour
{
    [SerializeField] public Sprite[] Auras;
    [SerializeField] public Sprite[] Bubbles;
    [SerializeField] public Image Bubble;
    [SerializeField] public Image Aura;

    private void Start()
    {
        int variantCount = Mathf.Min(Auras?.Length ?? 0, Bubbles?.Length ?? 0);
        if (variantCount == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, variantCount);
        if (Bubble != null)
        {
            Bubble.sprite = Bubbles[randomIndex];
        }

        if (Aura != null)
        {
            Aura.sprite = Auras[randomIndex];
        }
    }
}
