using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanVoteActionMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button blocker;
    [SerializeField] private RectTransform panel;

    [Header("Buttons")]
    [SerializeField] private Button roleVoteButton;
    [SerializeField] private Button kickVoteButton;

    private Canvas rootCanvas;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (blocker != null)
            blocker.onClick.AddListener(Close);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Open(RectTransform anchorButton, System.Action onRoleVote, System.Action onKickVote)
    {

        if (anchorButton == null) return;

        roleVoteButton.onClick.RemoveAllListeners();
        kickVoteButton.onClick.RemoveAllListeners();

        roleVoteButton.onClick.AddListener(() =>
        {
            onRoleVote?.Invoke();
            Close();
        });
        kickVoteButton.onClick.AddListener(() =>
        {
            onKickVote?.Invoke();
            Close();
        });

        Show();
        PositionNear(anchorButton);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        HideInstant();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideInstant()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void PositionNear(RectTransform anchorButton)
    {
        // 1) Counts the top-center point of the anchorButton in screen coordinates
        Vector3[] corners = new Vector3[4];
        anchorButton.GetWorldCorners(corners);

        Vector3 topCenterWorld = (corners[1] + corners[2]) * 0.5f;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            topCenterWorld
        );

        // 2) Changes screen point to local point in parent rect
        RectTransform parentRect = panel.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPoint,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector2 localPoint
        );

        // 3) Force layout rebuild to get correct size
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        // 4) Puts panel above the point with some padding
        float padding = 10f;
        panel.anchoredPosition = localPoint + new Vector2(0, padding);

        // 5) Clamp on parent rect
        ClampToRect(parentRect, panel);
    }

    private void ClampToRect(RectTransform container, RectTransform p)
    {
        Vector2 containerSize = container.rect.size;
        Vector2 panelSize = p.rect.size;

        Vector2 pos = p.anchoredPosition;
        Vector2 pivot = p.pivot;

        float halfW = containerSize.x * 0.5f;
        float halfH = containerSize.y * 0.5f;

        float minX = -halfW + panelSize.x * pivot.x;
        float maxX = halfW - panelSize.x * (1f - pivot.x);
        float minY = -halfH + panelSize.y * pivot.y;
        float maxY = halfH - panelSize.y * (1f - pivot.y);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        p.anchoredPosition = pos;
    }

}
