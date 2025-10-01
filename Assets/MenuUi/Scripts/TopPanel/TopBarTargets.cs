using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarTargets : MonoBehaviour
{
    public SettingsCarrier.TopBarStyle style;

    // targets[i] = IconsRoot children (direct)
    public List<GameObject> targets = new();

    // items[i] must match targets[i] 1:1
    public List<TopBarDefs.TopBarItem> items = new();

    [Header("Spacer (created if null)")]
    [SerializeField] private RectTransform _flexibleSpacer;
    [SerializeField] private float _spacerMinWidth = 0f;

    [SerializeField] private GameObject _standaloneLeaderboard;  // separate button under IconsRoot
    [SerializeField] private GameObject _clanTileLeaderboard;    // the LB button that lives inside the clan tile

    [SerializeField] private TopBarDefs.TopBarItem _clanItem = TopBarDefs.TopBarItem.ClanTile;
    [SerializeField] private TopBarDefs.TopBarItem _leaderboardItem = TopBarDefs.TopBarItem.Leaderboard;

    private void OnEnable()
    {
        ApplyFromSettings();
        SettingsCarrier.OnTopBarChanged += OnCarrierChanged;
    }

    private void OnDisable()
    {
        SettingsCarrier.OnTopBarChanged -= OnCarrierChanged;
    }

    private void OnCarrierChanged(int styleIndex)
    {
        if ((SettingsCarrier.TopBarStyle)styleIndex == style)
            ApplyFromSettings();
    }

    public void ApplyFromSettings()
    {
        if (!IsValid(out var parentRT)) return;

        // 1) Visibility from PlayerPrefs
        var vis = ReadVisibility();

        // 2) Special case: Clan vs Leaderboard
        ApplyClanLeaderboardRule(vis, out bool clanOn, out bool lbOn);

        // Activate/deactivate item targets
        for (int i = 0; i < targets.Count; i++)
            if (targets[i]) targets[i].SetActive(vis[i]);

        // Control the two leaderboard button variants
        if (_clanTileLeaderboard) _clanTileLeaderboard.SetActive(clanOn && lbOn);
        if (_standaloneLeaderboard) _standaloneLeaderboard.SetActive(lbOn && !clanOn);

        // 3) Order from PlayerPrefs (per style) and filter by visible
        var order = ReadOrder(vis);

        // 4) Place items with spacer
        ApplyOrderWithSpacer(parentRT, order);
    }

    private bool IsValid(out RectTransform parentRT)
    {
        parentRT = null;

        int count = targets.Count;
        if (count == 0 || items.Count != count)
        {
            Debug.LogWarning("TopBarTargets: 'items' length must match 'targets' length.");
            return false;
        }

        parentRT = targets[0]?.transform.parent as RectTransform;
        if (!parentRT)
        {
            Debug.LogWarning("TopBarTargets: parent RectTransform not found.");
            return false;
        }
        return true;
    }

    private string PrefKeyForItem(TopBarDefs.TopBarItem item)
    {
        // Safe even if TopBarDefs.Instance not yet initialized
        return TopBarDefs.Instance ? TopBarDefs.Instance.Key(item) : "TopBarItem_" + item;
    }

    private bool[] ReadVisibility()
    {
        int count = targets.Count;
        var vis = new bool[count];
        for (int i = 0; i < count; i++)
        {
            string key = PrefKeyForItem(items[i]);
            vis[i] = PlayerPrefs.GetInt(key, 1) != 0; // default ON
        }
        return vis;
    }

    private void ApplyClanLeaderboardRule(bool[] vis, out bool clanOn, out bool lbOn)
    {
        int clanIdx = items.IndexOf(_clanItem);
        int lbIdx = items.IndexOf(_leaderboardItem);

        clanOn = clanIdx >= 0 && vis[clanIdx];
        lbOn = lbIdx >= 0 && vis[lbIdx];

        // If both toggled on, hide the standalone LB item (but keep lbOn for the UI variant logic)
        if (clanOn && lbOn && lbIdx >= 0)
            vis[lbIdx] = false;
    }

    private List<int> ReadOrder(bool[] visible)
    {
        int count = targets.Count;

        // Parse CSV order
        string orderKey = "TopBarOrder_" + style;
        string csv = PlayerPrefs.GetString(orderKey, "");
        var used = new bool[count];
        var rawOrder = new List<int>(count);

        if (!string.IsNullOrEmpty(csv))
        {
            var parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int idx) && (uint)idx < (uint)count && !used[idx])
                {
                    used[idx] = true;
                    rawOrder.Add(idx);
                    if (rawOrder.Count == count) break;
                }
            }
        }

        // Append any missing indices
        for (int i = 0; i < count; i++)
            if (!used[i]) rawOrder.Add(i);

        // Keep only visible indices
        var filtered = new List<int>(rawOrder.Count);
        for (int i = 0; i < rawOrder.Count; i++)
        {
            int idx = rawOrder[i];
            if (visible[idx]) filtered.Add(idx);
        }

        return filtered;
    }

    // ----- Helpers: spacer & layout -----

    private void EnsureSpacer(RectTransform parent)
    {
        if (_flexibleSpacer == null)
        {
            var go = new GameObject("FlexibleSpacer", typeof(RectTransform), typeof(LayoutElement));
            _flexibleSpacer = go.GetComponent<RectTransform>();

            // Make sure it doesn't intercept input
            var cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
            cg.alpha = 0f;
        }

        if (_flexibleSpacer.parent != parent)
            _flexibleSpacer.SetParent(parent, false);

        var le = _flexibleSpacer.GetComponent<LayoutElement>();
        le.minWidth = _spacerMinWidth;
        le.preferredWidth = 0f;
        le.flexibleWidth = 1f; // push last item to right edge
        le.minHeight = 0f;
        le.preferredHeight = 0f;
        le.flexibleHeight = 0f;
    }

    private void ApplyOrderWithSpacer(RectTransform parentRT, List<int> orderedVisible)
    {
        int n = orderedVisible.Count;

        // 0 or 1 item => no spacer needed
        if (n <= 1)
        {
            if (n == 1) targets[orderedVisible[0]].transform.SetSiblingIndex(0);
            if (_flexibleSpacer) _flexibleSpacer.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
            return;
        }

        // Place all but the last
        int sib = 0;
        for (int i = 0; i < n - 1; i++)
            targets[orderedVisible[i]].transform.SetSiblingIndex(sib++);

        // Spacer
        EnsureSpacer(parentRT);
        _flexibleSpacer.gameObject.SetActive(true);
        _flexibleSpacer.SetSiblingIndex(sib++);

        // Last item (right-aligned thanks to spacer)
        targets[orderedVisible[n - 1]].transform.SetSiblingIndex(sib);

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
    }
}
