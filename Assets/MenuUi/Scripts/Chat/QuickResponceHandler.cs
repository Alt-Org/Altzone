using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickResponceHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;
    [SerializeField]
    private Button _button;

    public Button SetData(string text)
    {
        _text.text = text;
        return _button;
    }
}
