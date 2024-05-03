using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tallennetaan esineen tieto PlayerPrefsiin
            PlayerPrefs.SetInt("Interacted", 1);

            // Ladataan toinen scene
            SceneManager.LoadScene("KohdeScene");
        }
    }
}
