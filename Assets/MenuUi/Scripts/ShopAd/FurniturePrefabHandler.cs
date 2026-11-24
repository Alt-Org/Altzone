using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine.UI;
using UnityEngine;

public class FurniturePrefabHandler : AltMonoBehaviour
{
    [SerializeField] private Image _image;
    //[SerializeField] private Button _button;

    public void SetFurniture(AdFurnitureObject furniture)
    {
        _image.preserveAspect = true;
        _image.sprite = furniture.Image;
    }
}
