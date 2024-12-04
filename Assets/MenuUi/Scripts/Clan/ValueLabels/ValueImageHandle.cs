using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Clan;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ValueImageHandle : MonoBehaviour
{
    [SerializeField] private LabelReference _reference;
    [SerializeField] Image _image;

    public void SetLabelInfo(ClanValues value)
    {
        LabelInfoObject LabelInfo = _reference.GetLabelInfo(value);
        _image.sprite = LabelInfo.Image;
    }
}
