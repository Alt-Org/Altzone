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
    [SerializeField] private Transform _borderSelectionContent;
    [SerializeField] private GameObject _borderFramePrefab;
    private List<Image> _borderFrameList;

    private AdStoreObject _adData;
    private string _posterName = null;


    void Start()    
    {
        StartCoroutine(GetClanData(data=>
        {
            if (data != null)
            {
                if(data.AdData != null) _adData = data.AdData;
                else _adData = new(null, null);
                _posterName = data.Name;
            }
            else
            {
                _adData = new(null, null);
                _posterName = "Et ole klaanissa";
            }
            _adGraphicHandler.SetAdPoster(_adData, _posterName);
        }));


        List<AdBorderFrameObject> list = _borderReference.Info;

        foreach (AdBorderFrameObject frame in list)
        {
            GameObject frameObject = Instantiate(_borderFramePrefab, _borderSelectionContent);
            frameObject.GetComponent<Image>().sprite = frame.Image;
            float objectHeight= _borderSelectionContent.GetComponent<RectTransform>().rect.height * 0.9f;
            frameObject.GetComponent<RectTransform>().sizeDelta = new(objectHeight * 0.625f, objectHeight);
            frameObject.GetComponent<Button>().onClick.AddListener(() => ChangeBorder(frame));
        }

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
            StartCoroutine(SaveClanData(clanData => data = clanData, data));
        }));
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }


    public void ChangeOrangeColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(orangeColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangeYellowColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(yellowColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangeLightGreenColor()
    {
        _adData.BackgroundColour = "#"+ColorUtility.ToHtmlStringRGBA(lightGreenColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }

    public void ChangeLightBlueColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(lightBlueColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangeBlueColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(blueColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangePurpleColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(purpleColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangeDarkPinkColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(darkPinkColor);
        _adGraphicHandler.SetAdPoster(_adData, _posterName);
        SaveAdData();
    }
    public void ChangeRedColor()
    {
        _adData.BackgroundColour = "#" + ColorUtility.ToHtmlStringRGBA(redColor);
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
