// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TMPro;
using UnityEngine;

public class InputFieldSizeChange : MonoBehaviour
{

    [SerializeField]
    TMP_InputField _inputField;
    [SerializeField]
    TextMeshProUGUI _textMeshProUGUI;
    [SerializeField]
    RectTransform _rect;

    //[SerializeField]
    float PreferredHeight;

    private void OnEnable()
    {
        SetHeight();
    }

    public void SetHeight()
    {
        if (_textMeshProUGUI == null)
            return;

        PreferredHeight = _textMeshProUGUI.preferredHeight;
        if (PreferredHeight < 100)
        {
            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, 100);
        }
        else if (PreferredHeight > 100)
        {
            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, PreferredHeight + 10);
        }

        
    }

}
