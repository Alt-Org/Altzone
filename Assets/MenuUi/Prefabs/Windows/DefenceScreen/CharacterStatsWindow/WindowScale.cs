using System.Collections;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class WindowScale : MonoBehaviour
{
    public float _interval = 2f;// Kuinka usein ikkunan koon tarkistuksia suoritetaan (sekunteina).
    private SwipeUI _swipe; // Viittaus SwipeUI-komponenttiin.
    private GameObject[] _layoutElementsGameObjects; // Kaikki skaalautuvat käyttöliittymän elementit, joissa on tag "ScaleWindow".
    private RectTransform _scrollRectCanvas; // Viittaus ScrollRectCanvasin RectTransformiin

    // Muuttujat, joihin tallennetaan viimeisin tunnettu ikkunan leveys ja korkeus
    private int lastWidth;
    private int lastHeight;
    private SettingsCarrier carrier = SettingsCarrier.Instance; // Singleton-instanssi SettingsCarrier-luokasta
    private void Awake()
    {
        // Tallennetaan nykyinen ruudun leveys ja korkeus
        lastWidth = Screen.width; // Leveys
        lastHeight = Screen.height; // Korkeus
    }
    // OnEnable-metodi suoritetaan, kun komponentti aktivoituu
    private void OnEnable()
    {
        _swipe = GetComponentInParent<SwipeUI>(); // Haetaan SwipeUI-komponentti tämän objektin vanhemmasta (parent)
        StartCoroutine(CheckWindowSize()); // Tarkistaa ikkunan koon muutokset
    }
    void Start() // Start-metodi suoritetaan ennen ensimmäistä framepäivitystä
    {
        var windowManager = WindowManager.Get(); // Haetaan WindowManager-instanssi
        
        _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("ScaleWindow"); // Etsitään kaikki käyttöliittymän elementit, joissa on tag "ScaleWindow".
        // Haetaan käyttöliittymän elementti, joissa on tag "ScrollRectCanvas".
        _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
        SetMainMenuLayoutDimensions(); // Asetetaan päävalikon ulkoasun mitat
    }

    // Asettaa päävalikon käyttöliittymän mitat skaalaamalla elementtejä
    private void SetMainMenuLayoutDimensions()
    {
        Debug.Log("Setting dimensions");

        // Luodaan taulukko kaikista LayoutElement-komponenteista, jotka liittyvät skaalautuviin elementteihin
        LayoutElement[] layoutElements = new LayoutElement[_layoutElementsGameObjects.Length];

        // Käydään läpi jokainen käyttöliittymäelementti ja haetaan niistä LayoutElement-komponentti
        for (int i = 0; i < _layoutElementsGameObjects.Length; i++)
            layoutElements[i] = _layoutElementsGameObjects[i].GetComponent<LayoutElement>();

        // Haetaan ScrollRectCanvasin koko (leveys ja korkeus)
        float width = _scrollRectCanvas.sizeDelta.x;
        float height = _scrollRectCanvas.sizeDelta.y;

        // Asetetaan jokaiselle LayoutElement-komponentille leveys ja korkeus
        foreach (LayoutElement element in layoutElements)
        {
            element.preferredWidth = width;
            element.preferredHeight = height;
        }

        // Päivitetään SwipeUI:n pyyhkäisyalueen arvot
        _swipe.UpdateSwipeAreaValues();
    }

    // Korutiini, joka tarkistaa ikkunan koon muutokset säännöllisin väliajoin
    private IEnumerator CheckWindowSize()
    {
        while (true)
        {
            // Jos ruudun leveys tai korkeus on muuttunut, päivitetään layout-mitat
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                // Asetetaan päävalikon layoutin mitat uudelleen
                SetMainMenuLayoutDimensions();

                // Päivitetään SwipeUI:n tila
                _swipe.UpdateSwipe();

                // Päivitetään viimeisin tunnettu leveys ja korkeus
                lastWidth = Screen.width;
                lastHeight = Screen.height;
            }
            
            yield return new WaitForSeconds(_interval); // Odotetaan määritetty aika ennen seuraavaa tarkistusta
        }
    }
}
