using UnityEngine;
using UnityEngine.SceneManagement;

public class KylläButtonÄänestyst : MonoBehaviour
{
    // Tämä metodi kutsutaan, kun pelaaja painaa "Kyllä" -näppäintä
    public void YesButtonPressed()
    {
        // Tässä voit vaihtaa scenen tai siirtyä valikkoon
        SceneManager.LoadScene("ÄänestysView"); // Vaihda "UusiSceneNimi" haluamasi scenen nimeen
    }
}

    

