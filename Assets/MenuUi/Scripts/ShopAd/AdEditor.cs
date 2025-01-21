using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdEditor: MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private Image _effectImage;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _titleText;


    


    public void CloseEditor()
    {
        Destroy(gameObject);
    }
}
