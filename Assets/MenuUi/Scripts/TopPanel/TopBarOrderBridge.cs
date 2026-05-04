using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Settings; // TopBarDefs
using MenuUI.Scripts.TopPanel;

public class TopBarOrderBridge : MonoBehaviour
{
    [SerializeField] private RectTransform _toggleContainer;
    [SerializeField] private TopBarTargets[] _targetsByStyle;

    private const bool DebugOn = false;

    private SettingsCarrier.TopBarStyle CurrentStyle =>
        SettingsCarrier.Instance
            ? SettingsCarrier.Instance.TopBarStyleSetting
            : SettingsCarrier.TopBarStyle.NewHelena;

    private void OnEnable()
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : OnEnable()");

        if (_targetsByStyle == null || _targetsByStyle.Length == 0)
        {
            TopBarTargets[] found = FindObjectsOfType<TopBarTargets>(true);
            if (found != null && found.Length > 0) _targetsByStyle = found;
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

        foreach (Transform t in _toggleContainer)
        {
            TopBarToggleHandler h = t.GetComponentInChildren<TopBarToggleHandler>(true);
            if (h == null) continue;

            TopBarDefs.TopBarItem item = h.item;
            if (order.ContainsValue(item)) continue;
            order[nextPos] = item;
            nextPos++;
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
}
