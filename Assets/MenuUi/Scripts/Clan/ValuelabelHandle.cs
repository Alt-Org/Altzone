using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValuelabelHandle : MonoBehaviour
{
    [SerializeField]
    private LabelReference _reference;

    [SerializeField]
    private TextMeshProUGUI _textLabel;
    [SerializeField]
    private Image _labelImage;
    // Start is called before the first frame update
    void Start()
    {
        CheckLabelSize();
        
    }

    // Update is called once per frame
    void Update()
    {
        //CheckLabelSize();
    }
    public void SetLabelInfo(ClanValues value)
    {
       LabelInfoObject LabelInfo= _reference.GetLabelInfo(value);
        _textLabel.text = LabelInfo.Name;
        _labelImage.sprite = LabelInfo.Image;
    }
    public void CheckLabelSize()
    {
        float imagewidth=_labelImage.GetComponent<RectTransform>().sizeDelta.x;
        float imageleftpos = _labelImage.GetComponent<RectTransform>().localPosition.x;
        _textLabel.GetComponent<RectTransform>().offsetMin = new(imagewidth + imageleftpos + 10, _textLabel.GetComponent<RectTransform>().offsetMin.y);
    }
}
