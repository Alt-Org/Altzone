using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetColorOptionsButtonSizes : MonoBehaviour
{
    [SerializeField] private GameObject _referenceButton;
    [SerializeField] private GameObject[] _colorButtons;

    private void Start()
    {
        SetButtonSizes();
    }

    private void Update()
    {
#if UNITY_EDITOR
        SetButtonSizes();
#endif
    }

    private void SetButtonSizes()
    {
        Vector2 referenceSizeDelta = _referenceButton.GetComponent<RectTransform>().sizeDelta;

        foreach (var button in _colorButtons)
        {
            button.GetComponent<RectTransform>().sizeDelta = referenceSizeDelta;
        }
    }
}
