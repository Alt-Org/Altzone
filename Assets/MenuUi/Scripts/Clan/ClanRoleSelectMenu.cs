using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanRoleSelectMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button blocker;
    [SerializeField] private RectTransform panel;

    [Header("Template")]
    [SerializeField] private Button itemTemplate;
    [SerializeField] private Image templateIcon;
    [SerializeField] private TMP_Text templateLabel;

    [Header("Data")]
    [SerializeField] private ClanRoleCatalog roleCatalog;
    [SerializeField] private Sprite fallbackIcon;

    private Canvas rootCanvas;

    private readonly List<Button> spawned = new();

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (blocker != null)
        {
            blocker.onClick.AddListener(Close);
        }

        itemTemplate.gameObject.SetActive(false);
        HideInstant();
    }

    public void Open(RectTransform anchorButton, List<ClanRoles> roles, Action<ClanRoles> onPickRole)
    {
        Build(roles, onPickRole);

        Show();
        PositionNear(anchorButton);
        transform.SetAsLastSibling();
    }

    public void Close() => HideInstant();

    private void Build(List<ClanRoles> roles, Action<ClanRoles> onPickRole)
    {
        foreach (var b in spawned) Destroy(b.gameObject);
        spawned.Clear();

        if (roles == null) return;

        foreach (var role in roles)
        {
            var btn = Instantiate(itemTemplate, panel);
            btn.gameObject.SetActive(true);

            var icon = btn.transform.Find(templateIcon.name).GetComponent<Image>();
            var label = btn.transform.Find(templateLabel.name).GetComponent<TMP_Text>();

            var roleName = role.name;
            label.text = roleCatalog != null ? roleCatalog.GetDisplayName(roleName) : roleName;

            var sprite = roleCatalog != null ? roleCatalog.GetIcon(roleName) : null;
            icon.sprite = sprite != null ? sprite : fallbackIcon;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                onPickRole?.Invoke(role);
                Close();
            });

            spawned.Add(btn);
        }
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
        // same logic as in ClanVoteActionMenu
        Vector3[] corners = new Vector3[4];
        anchorButton.GetWorldCorners(corners);

        Vector3 topCenterWorld = (corners[1] + corners[2]) * 0.5f;

        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, topCenterWorld);

        RectTransform parentRect = panel.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out var localPoint);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        panel.anchoredPosition = localPoint + new Vector2(0, 10f);
    }
}
