// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TMPro;
using UnityEngine;
using System.Collections;

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
        _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, 100);
    }

    public IEnumerator SetHeight()
    {

        yield return new WaitForEndOfFrame();

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

    public void DoTheHeight()
    {
        StartCoroutine(SetHeight());
    }

}
