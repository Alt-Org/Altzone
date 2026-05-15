using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Settings; // TopBarDefs
using MenuUI.Scripts.TopPanel;
using UnityEngine.UI;


public class TopBarOrderBridge : MonoBehaviour
{
    [SerializeField] private RectTransform _toggleContainer;
    [SerializeField] private TopBarTargets[] _targetsByStyle;
    [SerializeField] private GameObject[] _clanSubItemSpacers;
    [SerializeField] private GameObject[] _clanSubItemRows;

    public static TopBarOrderBridge Active { get; private set; }
    private const bool DebugOn = true;

    private SettingsCarrier.TopBarStyle CurrentStyle =>
        SettingsCarrier.Instance
            ? SettingsCarrier.Instance.TopBarStyleSetting
            : SettingsCarrier.TopBarStyle.NewHelena;

    private void OnEnable()
    {
        Active = this;

        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : OnEnable()");

        if (_targetsByStyle == null || _targetsByStyle.Length == 0)
        {
            // TopBarTargets[] found = FindObjectsOfType<TopBarTargets>(true);
            // if (found != null && found.Length > 0) _targetsByStyle = found;

            Debug.LogWarning("[TB] Targets By Style is empty. Assign the correct TopPanel Alt1 manually in Inspector.");
            return;
        }

        SetRowDropEventSubscriptions(true);
        UpdateTopBarStyle(CurrentStyle);
        SettingsCarrier.OnTopBarChanged += HandleCarrierChanged;
    }

    private void OnDisable()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : OnDisable()");

        SettingsCarrier.OnTopBarChanged -= HandleCarrierChanged;
        SetRowDropEventSubscriptions(false);
    }

    private void HandleCarrierChanged(int styleIndex)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : HandleCarrierChanged()");

        UpdateTopBarStyle((SettingsCarrier.TopBarStyle)styleIndex);
    }

    private void SetRowDropEventSubscriptions(bool subscribe)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : SetRowDropEventSubscriptions()");

        if (_toggleContainer == null) return;

        TopBarToggleDrag[] handles = _toggleContainer.GetComponentsInChildren<TopBarToggleDrag>(true);
        for (int i = 0; i < handles.Length; i++)
        {
            if (subscribe) handles[i].OnDropped += OnRowDropped;
            else handles[i].OnDropped -= OnRowDropped;
        }
    }

    private TopBarTargets GetTargetsFor(SettingsCarrier.TopBarStyle style)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : GetTargetsFor()");

        if (_targetsByStyle == null) return null;
        for (int i = 0; i < _targetsByStyle.Length; i++)
        {
            TopBarTargets t = _targetsByStyle[i];
            if (t != null && t.style == style) return t;
        }

        return null;
    }

    private void OnRowDropped()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : OnRowDropped()");

        TopBarTargets owner = GetTargetsFor(CurrentStyle);
        if (owner == null || _toggleContainer == null) return;

        Dictionary<int, TopBarDefs.TopBarItem> order =
            new Dictionary<int, TopBarDefs.TopBarItem>(_toggleContainer.childCount);
        int nextPos = 0;

        bool clanTileOn = PlayerPrefs.GetInt(
            TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile) + "_" + CurrentStyle,
            1
        ) != 0;

        foreach (Transform t in _toggleContainer)
        {
            TopBarToggleHandler h = t.GetComponentInChildren<TopBarToggleHandler>(true);
            if (h == null) continue;

            TopBarDefs.TopBarItem item = h.item;

            bool isClanSubItem =
                item == TopBarDefs.TopBarItem.Leaderboard ||
                item == TopBarDefs.TopBarItem.ClanLogo ||
                item == TopBarDefs.TopBarItem.ClanTextContainer ||
                item == TopBarDefs.TopBarItem.Coins;

            if (clanTileOn && isClanSubItem)
                continue;

            if (order.ContainsValue(item)) continue;

            order[nextPos] = item;
            nextPos++;

            if (clanTileOn && item == TopBarDefs.TopBarItem.ClanTile)
            {
                order[nextPos++] = TopBarDefs.TopBarItem.Leaderboard;
                order[nextPos++] = TopBarDefs.TopBarItem.Coins;
                order[nextPos++] = TopBarDefs.TopBarItem.ClanTextContainer;
                order[nextPos++] = TopBarDefs.TopBarItem.ClanLogo;
            }
        }

        int total = owner.RowCount();
        for (int i = 0; i < total; i++)
        {
            TopBarDefs.TopBarItem it = owner.GetItemAt(i);
            if (!order.ContainsValue(it))
            {
                order[nextPos] = it;
                nextPos++;
            }
        }

        List<int> positions = new List<int>(order.Keys);
        positions.Sort();

        List<int> indices = new List<int>(positions.Count);
        foreach (int pos in positions)
        {
            TopBarDefs.TopBarItem item;
            if (!order.TryGetValue(pos, out item)) continue;

            int idx = -1;
            for (int k = 0; k < total; k++)
            {
                if (owner.GetItemAt(k).Equals(item))
                {
                    idx = k;
                    break;
                }
            }

            if (idx >= 0 && !indices.Contains(idx)) indices.Add(idx);
        }

        for (int i = 0; i < total; i++)
            if (!indices.Contains(i))
                indices.Add(i);

        SettingsCarrier instance = SettingsCarrier.Instance;

        Debug.Log("[TopBarDebugOn] SAVING ORDER INDICES:");
        foreach (int idx in indices)
        {
            Debug.Log($"[TopBarDebugOn] : save idx={idx}, item={owner.GetItemAt(idx)}");
        }

        if (instance != null)
        {
            instance.SaveTopBarOrder(owner.style, indices);
            owner.ApplyOrderFromSettings();
        }
        else
        {
            Debug.LogWarning("[TopBarOrderBridge] SettingsCarrier instance is null");
        }

        ApplyOrderToToggleList(order, _toggleContainer, owner);

        SetClanSubItemIndent(
            PlayerPrefs.GetInt(
                TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile) + "_" + CurrentStyle,
                1
            ) != 0
        );
    }

    private void UpdateTopBarStyle(SettingsCarrier.TopBarStyle style)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : UpdateTopBarStyle()");

        TopBarTargets owner = GetTargetsFor(style);
        if (owner == null || _toggleContainer == null) return;

        int total = owner.RowCount();

        List<int> orderList = SettingsCarrier.LoadTopBarOrderStatic(style, total);

        Dictionary<int, TopBarDefs.TopBarItem> order = new Dictionary<int, TopBarDefs.TopBarItem>(orderList.Count);
        int pos = 0;

        foreach (int idx in orderList)
        {
            if ((uint)idx >= (uint)total) continue;
            TopBarDefs.TopBarItem item = owner.GetItemAt(idx);
            if (order.ContainsValue(item)) continue;
            order[pos] = item;
            pos++;
        }

        for (int i = 0; i < total; i++)
        {
            TopBarDefs.TopBarItem it = owner.GetItemAt(i);
            if (!order.ContainsValue(it))
            {
                order[pos] = it;
                pos++;
            }
        }

        ApplyOrderToToggleList(order, _toggleContainer, owner);
        owner.ApplyFromSettings();
        owner.ApplyOrderFromSettings();

        bool clanTileOn = PlayerPrefs.GetInt(
            TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile) + "_" + style,
            1
        ) != 0;

        SetClanSubItemIndent(clanTileOn);
    }

    private static void ApplyOrderToToggleList(
        Dictionary<int, TopBarDefs.TopBarItem> order,
        RectTransform container,
        TopBarTargets owner)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : ApplyOrderToToggleList()");

        Dictionary<TopBarDefs.TopBarItem, RectTransform> rowOf =
            new Dictionary<TopBarDefs.TopBarItem, RectTransform>(container.childCount);

        foreach (Transform t in container)
        {
            TopBarToggleHandler h = t.GetComponentInChildren<TopBarToggleHandler>(true);
            if (h == null) continue;
            if (!rowOf.ContainsKey(h.item)) rowOf.Add(h.item, (RectTransform)t);
        }

        List<int> positions = new List<int>(order.Keys);
        positions.Sort();

        int sibling = 0;
        foreach (int pos in positions)
        {
            TopBarDefs.TopBarItem item;
            if (!order.TryGetValue(pos, out item)) continue;

            RectTransform rt;
            if (rowOf.TryGetValue(item, out rt))
            {
                rt.SetSiblingIndex(sibling);
                sibling++;
            }
        }
    }

    private void SetClanSubItemIndent(bool clanTileOn)
    {
        Debug.Log($"[TB] SetClanSubItemIndent clanTileOn={clanTileOn}, count={_clanSubItemSpacers?.Length}");

        if (_clanSubItemSpacers == null) return;

        foreach (GameObject spacer in _clanSubItemSpacers)
        {
            Debug.Log($"[TB] spacer={spacer?.name}, setActive={clanTileOn}");
            if (spacer != null)
                spacer.SetActive(clanTileOn);
        }

        SetClanSubItemRowsLocked(clanTileOn);
    }

    public void RefreshClanSubItemIndent()
    {
        SetClanSubItemIndent(
            PlayerPrefs.GetInt(
                TopBarDefs.Key(TopBarDefs.TopBarItem.ClanTile) + "_" + CurrentStyle,
                1
            ) != 0
        );
    }

    private void SetClanSubItemRowsLocked(bool clanTileOn)
    {
        if (_clanSubItemRows == null) return;

        foreach (GameObject row in _clanSubItemRows)
        {
            if (row == null) continue;

            TopBarToggleDrag drag = row.GetComponent<TopBarToggleDrag>();
            if (drag != null)
                drag.enabled = !clanTileOn;

            Toggle toggle = row.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                // toggle.interactable = !clanTileOn;
                toggle.interactable = true;
            }
        }
    }

    public void ApplyCurrentTarget()
    {
        TopBarTargets owner = GetTargetsFor(CurrentStyle);

        if (owner == null)
        {
            Debug.LogWarning("[TB] No owner found");
            return;
        }

        Debug.Log($"[TB] APPLY CURRENT TARGET => {owner.name}");

        owner.ApplyFromSettings();
        RefreshClanSubItemIndent();
    }
}
