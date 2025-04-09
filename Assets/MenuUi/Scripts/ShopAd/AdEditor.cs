using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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


    void Start()    
    {
        StartCoroutine(GetClanData(data=>
        {
            if (data != null)
            {
                clanNameText.text = (data.Name);
            }
            else
            {
                clanNameText.text = "Et ole klaanissa";
            }
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
        yield return null;
        foreach (Transform frame in _borderSelectionContent)
        {
            float objectHeight = _borderSelectionContent.GetComponent<RectTransform>().rect.height * 0.9f;
            frame.GetComponent<RectTransform>().sizeDelta = new(objectHeight * 0.625f, objectHeight);
        }
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }


    public void ChangeOrangeColor()
    {
        _backgroundImage.color = orangeColor;
    }
    public void ChangeYellowColor()
    {
        _backgroundImage.color = yellowColor;
    }
    public void ChangeLightGreenColor()
    {
        _backgroundImage.color = lightGreenColor;
    }

    public void ChangeLightBlueColor()
    {
        _backgroundImage.color = lightBlueColor;
    }
    public void ChangeBlueColor()
    {
        _backgroundImage.color = blueColor;
    }
    public void ChangePurpleColor()
    {
        _backgroundImage.color = purpleColor;
    }
    public void ChangeDarkPinkColor()
    {
        _backgroundImage.color = darkPinkColor;
    }
    public void ChangeRedColor()
    {
        _backgroundImage.color = redColor;
    }

    public void ChangeBorder(AdBorderFrameObject frame)
    {
        _borderImage.sprite = frame.Image;
    }

    public void CloseEditor()
    {
        gameObject.SetActive(false);
    }
}
