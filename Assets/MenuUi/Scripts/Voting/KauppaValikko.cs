// KauppaValikko.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class KauppaValikko : MonoBehaviour
{
    public GameObject aanestysValikko; // Äänestysvalikko-objekti Unityssa
    private Kohde valittuKohde; // Tallentaa valitun kohteen tiedot

    

    public void ValitseKohde(Kohde kohde)
    {
        valittuKohde = kohde; // Tallenna valittu kohde
    }

    public void KyllaNappiOnClick()
    {
        // Varmista, että äänestysvalikko-objekti on määritelty
        if (aanestysValikko != null && valittuKohde != null)
        {
            


            // Aseta äänestysvalikon kuvan viite valitun kohteen kuvaan
            //aanestysValikko.GetComponent<AanestysValikko>().AsetaKuva(valittuKohde.kuva);
            //// Vaihda scene äänestysvalikkoon
            //SceneManager.LoadScene("AanestysScene");
        }
    }
}
