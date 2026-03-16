using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KaikkiOnHyvinPopupScript : MonoBehaviour
{
    [SerializeField] 
    private GameObject popup;
    [SerializeField] 
    private TextMeshProUGUI popupText;
    [SerializeField] private 
    Button button;
    private float transitionDuration = 0.4F;
    public static KaikkiOnHyvinPopupScript Instance; // static variable

    private string[] messages = 
    {
    "En yliajatellut tarpeeksi.",
    "AAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
    "Ei mennyt niin kuin Strömsössä. Ehkä Suomen Surkein Kuski ois meikäläisen juttu.",
    "Siis ei vaa millää jaksais.",
    "Aikainen lintu madon nappaa. Minä olen mato.",
    "Pitäs vaan luovuttaa.",
    "Alamäki jatkuu.",
    "Turhaan tässä yrittää.",
    "Ei mistään tuu mitään.",
    "Ei oo maailman loppu? NO KYLLÄ ON!",
    "Lasi on puoliksi tyhjä.",
    "Pettymys pettymyksen jälkee.",
    "Kolmas kerta todensanoo, mut tää onkin jo kolmaskymmenes kerta.",
    "Kurkotin kuusentaimeen, ja silti katajaan kapsahdin.",
    "Itken markkinoilla.",
    "Minkä taakseen jättää, sen edestään löytää ja nyt tieni on tukossa.",
    "Jään tuleen makaamaan.",
    "Ruoho on haaleaa aidan molemmilla puolilla.",
    "Nuolaisin ennen kuin tipahtaa ja tipahdin itse."
    };
    string message = "";

    public void popupController() 
    { 
        int randomNumber = Random.Range(0, messages.Length);
        message = messages[randomNumber];
        popupText.text = $"'{message}'";
        popup.SetActive(true);
        StartCoroutine(popupAnimation());

        button.onClick.AddListener(() =>{
            hidePopup();
        });
    }

    void Start()
    {
        popup.SetActive(false);
    }

    private IEnumerator popupAnimation()
    {    
        float elapsedTime = 0f;
        float scale = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            scale = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration);
            this.transform.localScale = new Vector3(scale, scale, 1);

            yield return null;
        }
    }

    public void hidePopup()
    {
        popup.SetActive(false);
    }
    void Awake()
    {
        Instance = this; // sets Instance to this script: can be found without a reference set in inspector
    }
}
