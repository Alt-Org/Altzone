using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// [ExecuteInEditMode]
[RequireComponent(typeof(LayoutElement))]
public class BattlePopupHeightSetter : MonoBehaviour
{
    [SerializeField] private float _aspectRatio;

    private void Start() => SetHeight();

    private void SetHeight()
    {
        if (_aspectRatio <= 0) return;

        // Layout groups take normally one frame to update -> force to get correct width values
        Canvas.ForceUpdateCanvases();

        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();

        float h = parentRect.rect.width / _aspectRatio;
        GetComponent<LayoutElement>().preferredHeight = h;
    }
}
