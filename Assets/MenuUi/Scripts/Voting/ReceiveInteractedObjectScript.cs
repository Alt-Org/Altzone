using UnityEngine;

public class ReceiveInteractedObjectScript : MonoBehaviour
{
    void Start()
    {
        // Tarkistetaan, onko esineeseen interaktoitu toisessa sceness‰
        if (PlayerPrefs.GetInt("Interacted") == 1)
        {
            // Suoritetaan toiminta esineen tietojen perusteella
            // Esimerkiksi voit n‰ytt‰‰ kuvan tai suorittaa muun toiminnon
        }
    }
}
