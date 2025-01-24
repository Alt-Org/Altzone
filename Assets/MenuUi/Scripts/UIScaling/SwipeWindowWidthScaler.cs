using System.Collections;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class SwipeWindowWidthScaler : MonoBehaviour
{   
    // Kuinka usein ikkunan koon tarkistuksia suoritetaan (sekunteina).
    // How often window size checks are performed (in seconds).
    public float _interval = 2f; 
    // Viittaus SwipeUI-komponenttiin. // Reference to the SwipeUI component.
    [SerializeField] private SwipeUI _swipe; 
    // Kaikki skaalautuvat käyttöliittymän elementit, joissa on tag "ScaleWindow". // All scalable UI elements tagged with "ScaleWindow".
    private GameObject[] _layoutElementsGameObjects;
    // Viittaus ScrollRectCanvasin RectTransformiin. // Reference to the RectTransform of the ScrollRectCanvas.
    private RectTransform _scrollRectCanvas; 

    // Muuttujat, joihin tallennetaan viimeisin tunnettu ikkunan leveys ja korkeus. 
    // Variables to store the last known screen width and height.
    private int lastWidth;
    private int lastHeight;
    private SettingsCarrier carrier = SettingsCarrier.Instance;

    private void Awake()
    {
        // Tallennetaan nykyinen ruudun leveys ja korkeus. 
        // Store the current screen width and height.
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    // OnEnable-metodi suoritetaan, kun komponentti aktivoituu. 
    // OnEnable method is called when the component is enabled.
    private void OnEnable()
    {
        // Haetaan SwipeUI-komponentti tämän objektin vanhemmasta (parent). 
        // Retrieve the SwipeUI component from this object's parent.
        if(_swipe == null) _swipe = GetComponentInParent<SwipeUI>(); 
        // Tarkistaa ikkunan koon muutokset. 
        // Start checking for window size changes.
        StartCoroutine(CheckWindowSize()); 
    }

    // Start-metodi suoritetaan ennen ensimmäistä framepäivitystä. 
    // Start method is executed before the first frame update.
    void Start()
    {
        // Haetaan WindowManager-instanssi. 
        // Retrieve the WindowManager instance.
        var windowManager = WindowManager.Get(); 

        // Etsitään kaikki käyttöliittymän elementit, joissa on tag "ScaleWindow". 
        // Find all UI elements tagged with "ScaleWindow".
        _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("ScaleWindow");

        // Haetaan käyttöliittymän elementti, jossa on tag "ScrollRectCanvas". 
        // Find the UI element tagged with "ScrollRectCanvas" and get its RectTransform component.
        _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
        // Asetetaan päävalikon ulkoasun mitat. 
        // Set the dimensions of the main menu layout.
        SetMainMenuLayoutDimensions(); 
    }

    // Asettaa päävalikon käyttöliittymän mitat skaalaamalla elementtejä. 
    // Sets the dimensions of the main menu layout by scaling elements.
    private void SetMainMenuLayoutDimensions()
    {
        Debug.Log("Setting dimensions");

        // Luodaan taulukko kaikista LayoutElement-komponenteista, jotka liittyvät skaalautuviin elementteihin. 
        // Create an array of LayoutElement components associated with the scalable elements.
        LayoutElement[] layoutElements = new LayoutElement[_layoutElementsGameObjects.Length];

        // Käydään läpi jokainen käyttöliittymäelementti ja haetaan niistä LayoutElement-komponentti. 
        // Iterate through each UI element and retrieve its LayoutElement component.
        for (int i = 0; i < _layoutElementsGameObjects.Length; i++)
            layoutElements[i] = _layoutElementsGameObjects[i].GetComponent<LayoutElement>();

        // Haetaan ScrollRectCanvasin koko (leveys ja korkeus). 
        // Retrieve the size (width and height) of the ScrollRectCanvas.
        float width = _scrollRectCanvas.sizeDelta.x;
        float height = _scrollRectCanvas.sizeDelta.y;

        // Asetetaan jokaiselle LayoutElement-komponentille leveys ja korkeus. 
        // Set the preferred width and height for each LayoutElement component.
        foreach (LayoutElement element in layoutElements)
        {
            element.preferredWidth = width;
            element.preferredHeight = height;
        }

        // Päivitetään SwipeUI:n pyyhkäisyalueen arvot. 
        // Update the swipe area values of the SwipeUI.
        _swipe.UpdateSwipeAreaValues();
    }

    // Tarkistaa ikkunan koon muutokset säännöllisin väliajoin. 
    // Periodically checks for window size changes.
    private IEnumerator CheckWindowSize()
    {
        while (true)
        {
            // Jos ruudun leveys tai korkeus on muuttunut, päivitetään layout-mitat. 
            // If the screen width or height has changed, update the layout dimensions.
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                // Asetetaan päävalikon layoutin mitat uudelleen. 
                // Reset the main menu layout dimensions.
                SetMainMenuLayoutDimensions();

                // Päivitetään SwipeUI:n tila. 
                // Update the state of SwipeUI.
                _swipe.UpdateSwipe();

                // Päivitetään viimeisin tunnettu leveys ja korkeus. 
                // Update the last known width and height.
                lastWidth = Screen.width;
                lastHeight = Screen.height;
            }
            // Odotetaan määritetty aika ennen seuraavaa tarkistusta. 
            // Wait for the specified interval before the next check.
            yield return new WaitForSeconds(_interval); 
        }
    }
}
