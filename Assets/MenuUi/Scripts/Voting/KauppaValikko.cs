// KauppaValikko.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class KauppaValikko : MonoBehaviour
{
    public GameObject aanestysValikko; // ƒ‰nestysvalikko-objekti Unityssa
    private Kohde valittuKohde; // Tallentaa valitun kohteen tiedot

    public void ValitseKohde(Kohde kohde)
    {
        valittuKohde = kohde; // Tallenna valittu kohde
    }

    public void KyllaNappiOnClick()
    {
        // Varmista, ett‰ ‰‰nestysvalikko-objekti on m‰‰ritelty
        if (aanestysValikko != null && valittuKohde != null)
        {
            // Aseta ‰‰nestysvalikon kuvan viite valitun kohteen kuvaan
            aanestysValikko.GetComponent<AanestysValikko>().AsetaKuva(valittuKohde.kuva);
            // Vaihda scene ‰‰nestysvalikkoon
            SceneManager.LoadScene("AanestysScene");
        }
    }
}
