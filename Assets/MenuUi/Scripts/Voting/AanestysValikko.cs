// AanestysValikko.cs
using UnityEngine;
using UnityEngine.UI;

public class AanestysValikko : MonoBehaviour
{
    public Image kuvaObjekti; // Unityn Image-objekti, johon kuva n‰ytet‰‰n

    public void AsetaKuva(Sprite kuva)
    {
        kuvaObjekti.sprite = kuva; // Aseta kuva kuvaObjektiin
    }
}
