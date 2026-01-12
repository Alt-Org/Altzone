using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskImageHandler : MonoBehaviour
{
    [SerializeField] private Image _bodySlotMask;

    public void SetImage(Sprite bodyImage)
    {
        _bodySlotMask.sprite = bodyImage;
    }
}
