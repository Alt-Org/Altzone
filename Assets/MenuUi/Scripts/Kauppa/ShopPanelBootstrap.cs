using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelBootstrap : MonoBehaviour
{
    private List<RectTransform> _rectTransforms;
    private void Awake()
    {
        _rectTransforms = new();
    }
    private void OnEnable()
    {
        _rectTransforms.Clear();
        FlexibleHorizontalGrid[] horizontalGrids = GetComponentsInChildren<FlexibleHorizontalGrid>();
        for (int i = 0; i < horizontalGrids.Length; i++)
        {
            RectTransform rect = horizontalGrids[i].GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        Debug.Log("Onenable");
    }
}
