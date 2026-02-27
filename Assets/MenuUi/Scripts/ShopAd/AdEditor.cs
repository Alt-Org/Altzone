using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Store;

public class AdEditor : AltMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clanNameText;

    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _effectImage;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private AdPosterHandler _adGraphicHandler;

    [SerializeField] private Color orangeColor;
    [SerializeField] private Color yellowColor;
    [SerializeField] private Color lightGreenColor;
    [SerializeField] private Color lightBlueColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color purpleColor;
    [SerializeField] private Color darkPinkColor;
    [SerializeField] private Color redColor;

    [SerializeField] private AdDecorationReference _borderReference;
    [Header("Frame Selectors")]
    [SerializeField] private Transform _borderSelectionContent;
    [SerializeField] private GameObject _borderFramePrefab;
    [SerializeField] private DailyTaskSelectButtons _dtSelectButtons;
    [Header("Colour Selectors")]
    [SerializeField] private Transform _backgroundColourSelectorContent;
    [SerializeField] private GameObject _backgroundColourSelectorPrefab;

    private AdStoreObject _adData;
    private string _posterName = null;
    private List<HeartPieceData> _heartPieceData = new();


    void Start()    
    {
        InitializeAd();
    }

    private void InitializeAd()
    {
        StartCoroutine(GetClanData(data =>
        {
            if (data != null)
            {
                if (data.AdData != null) _adData = data.AdData;
                else _adData = new(null, null);
                _posterName = data.Name;
                _heartPieceData = data.ClanHeartPieces;
            }
            else
            {
                _adData = new(null, null);
                _posterName = "Et ole klaanissa";
            }
            _adGraphicHandler.SetAdPoster(_adData, _posterName, _heartPieceData);
        }));


        List<AdBorderFrameObject> frameList = _borderReference.FrameList;

        foreach (AdBorderFrameObject frame in frameList)
        {
            GameObject frameObject = Instantiate(_borderFramePrefab, _borderSelectionContent);
            frameObject.GetComponent<Image>().sprite = frame.Image;
            float objectHeight = _borderSelectionContent.GetComponent<RectTransform>().rect.height * 0.9f;
            frameObject.GetComponent<RectTransform>().sizeDelta = new(objectHeight * 0.625f, objectHeight);
            frameObject.GetComponent<Button>().onClick.AddListener(() => ChangeBorder(frame));
            if(_dtSelectButtons)_dtSelectButtons.AddButton(new(frameObject.GetComponent<Button>(), frameObject.GetComponent<Image>()));
        }
        if (_dtSelectButtons) _dtSelectButtons.RefreshListeners();

        List<Color> colorList = _borderReference.ColourList;

        foreach (Color colour in colorList)
        {
            GameObject colourObject = Instantiate(_backgroundColourSelectorPrefab, _backgroundColourSelectorContent);
            colourObject.GetComponent<Image>().color = colour;
            float objectWidth = _backgroundColourSelectorContent.GetComponent<RectTransform>().rect.width;
            colourObject.GetComponent<RectTransform>().sizeDelta = new(objectWidth, objectWidth * 0.4f);
            colourObject.GetComponent<Button>().onClick.AddListener(() => ChangeColor(colour));
        }
        _backgroundColourSelectorContent.GetComponent<VerticalLayoutGroup>().spacing = _backgroundColourSelectorContent.GetComponent<RectTransform>().rect.width * 0.1f;

        StartCoroutine(SetFrameSelectionSize());
    }

    private IEnumerator SetFrameSelectionSize()
    {
        yield return new WaitForEndOfFrame();
        foreach (Transform frame in _borderSelectionContent)
        {
            float objectHeight = _borderSelectionContent.GetComponent<RectTransform>().rect.height * 0.9f;
            frame.GetComponent<RectTransform>().sizeDelta = new(objectHeight * 0.625f, objectHeight);
        }
    }

    private void SaveAdData()
    {
        StartCoroutine(GetClanData(data =>
        {
            data.AdData = _adData;
            StartCoroutine(ServerManager.Instance.UpdateClanAdPoster(_adData, success =>
            {
                if (success)
                {
                    StartCoroutine(SaveClanData(clanData => data = clanData, data));
                }
            }));
        }));
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }

    public void ChangeColor(Color colour)
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(colour);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }

    public void ChangeBorder(AdBorderFrameObject frame)
    {
        _adData.BorderFrame = frame.Name;
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
