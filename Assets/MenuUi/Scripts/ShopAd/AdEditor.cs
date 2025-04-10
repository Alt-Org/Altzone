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


    void Start()    
    {
        StartCoroutine(GetClanData(data=>
        {
            if (data != null)
            {
                clanNameText.text = (data.Name);
                if(data.AdData != null) _adData = data.AdData;
                else _adData = new(null, null);
            }
            else
            {
                _adData = new(null, null);
                clanNameText.text = "Et ole klaanissa";
            }
            _borderImage.sprite = AdDecorationReference.Instance.GetBorderFrameSprite(_adData.BorderFrame);
            if(ColorUtility.TryParseHtmlString(_adData.BackgroundColour, out Color colour)) _backgroundImage.color = colour;
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
        _backgroundImage.color = orangeColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(orangeColor);
        SaveAdData();
    }
    public void ChangeYellowColor()
    {
        _backgroundImage.color = yellowColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(yellowColor);
        SaveAdData();
    }
    public void ChangeLightGreenColor()
    {
        _backgroundImage.color = lightGreenColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(lightGreenColor);
        SaveAdData();
    }

    public void ChangeLightBlueColor()
    {
        _backgroundImage.color = lightBlueColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(lightBlueColor);
        SaveAdData();
    }
    public void ChangeBlueColor()
    {
        _backgroundImage.color = blueColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(blueColor);
        SaveAdData();
    }
    public void ChangePurpleColor()
    {
        _backgroundImage.color = purpleColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(purpleColor);
        SaveAdData();
    }
    public void ChangeDarkPinkColor()
    {
        _backgroundImage.color = darkPinkColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(darkPinkColor);
        SaveAdData();
    }
    public void ChangeRedColor()
    {
        _backgroundImage.color = redColor;
        _adData.BackgroundColour = ColorUtility.ToHtmlStringRGBA(redColor);
        SaveAdData();
    }

    public void ChangeBorder(AdBorderFrameObject frame)
    {
        _borderImage.sprite = frame.Image;
        _adData.BorderFrame = frame.Name;
        SaveAdData();
    }

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
