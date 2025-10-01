using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarOrderBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform _toggleContainer;          // The gray list (TopBarToggles)
    [Header("[0]=Old, [1]=NewHelena, [2]=NewNiko")]
    [SerializeField] private TopBarTargets[] _targetsByStyle;         // One per style

    private SettingsCarrier.TopBarStyle CurrentStyle =>
        SettingsCarrier.Instance ? SettingsCarrier.Instance.TopBarStyleSetting
                                 : SettingsCarrier.TopBarStyle.NewHelena;

    private void OnEnable()
    {
        HookRows(true);
        ApplyFromSettingsFor(CurrentStyle);
        SettingsCarrier.OnTopBarChanged += HandleCarrierChanged;
    }

    private void OnDisable()
    {
        SettingsCarrier.OnTopBarChanged -= HandleCarrierChanged;
        HookRows(false);
    }

    private void HandleCarrierChanged(int styleIndex)
    {
        ApplyFromSettingsFor((SettingsCarrier.TopBarStyle)styleIndex);
    }

    // Subscribe/unsubscribe all drag rows once
    private void HookRows(bool add)
    {
        if (!_toggleContainer) return;

        var rows = _toggleContainer.GetComponentsInChildren<TopBarToggleDrag>(true);
        foreach (var r in rows)
        {
            if (add) r.OnDropped += OnRowDropped;
            else     r.OnDropped -= OnRowDropped;
        }
    }

    private void OnRowDropped()
    {
        var targets = GetTargetsFor(CurrentStyle);
        if (!targets || !_toggleContainer) return;

        var newOrder = BuildOrderFromContainer(_toggleContainer, targets.items);

        // Persist ? this also fires OnTopBarChanged that TopBarTargets listens to
        SettingsCarrier.Instance?.SaveTopBarOrder(targets.style, newOrder);

        // Update the settings list immediately for a snappy feel
        ApplyOrderToToggleList(newOrder, _toggleContainer, targets.items);
    }

    private void ApplyFromSettingsFor(SettingsCarrier.TopBarStyle style)
    {
        var targets = GetTargetsFor(style);
        if (!targets || !_toggleContainer) return;

        int count = targets.items.Count;

        // Static read to avoid instance/timing dependency
        var order = SettingsCarrier.LoadTopBarOrderStatic(style, count);

        ApplyOrderToToggleList(order, _toggleContainer, targets.items);
        targets.ApplyFromSettings();
    }


    // ---------- Helpers ----------

    private TopBarTargets GetTargetsFor(SettingsCarrier.TopBarStyle style)
    {
        int i = (int)style;
        return (_targetsByStyle != null && i >= 0 && i < _targetsByStyle.Length) ? _targetsByStyle[i] : null;
    }

    // Build a permutation [index…] based on current sibling order in the toggle container
    private static List<int> BuildOrderFromContainer(
        RectTransform container,
        List<TopBarDefs.TopBarItem> itemsMap)
    {
        var order = new List<int>(itemsMap.Count);

        // Precompute enum ? index for O(1) lookup
        var indexOf = BuildItemIndexMap(itemsMap);
        var used = new bool[itemsMap.Count];

        // Walk direct children in sibling order
        for (int i = 0; i < container.childCount; i++)
        {
            var row = container.GetChild(i);
            var handler = row.GetComponent<TopBarToggleHandler>();
            if (!handler) continue;

            if (indexOf.TryGetValue(handler.item, out int idx) && !used[idx])
            {
                used[idx] = true;
                order.Add(idx);
            }
        }

        // Fill any missing indices to complete a valid permutation
        for (int i = 0; i < itemsMap.Count; i++)
            if (!used[i]) order.Add(i);

        return order;
    }

    private static Dictionary<TopBarDefs.TopBarItem, int> BuildItemIndexMap(List<TopBarDefs.TopBarItem> itemsMap)
    {
        var map = new Dictionary<TopBarDefs.TopBarItem, int>(itemsMap.Count);
        for (int i = 0; i < itemsMap.Count; i++) map[itemsMap[i]] = i;
        return map;
    }

    // Arrange the toggle rows’ sibling indices to match `order`
    private static void ApplyOrderToToggleList(
        List<int> order,
        RectTransform container,
        List<TopBarDefs.TopBarItem> itemsMap)
    {
        // Build item ? row map once
        var rowOf = new Dictionary<TopBarDefs.TopBarItem, RectTransform>(itemsMap.Count);
        for (int i = 0; i < container.childCount; i++)
        {
            var row = container.GetChild(i);
            var handler = row.GetComponent<TopBarToggleHandler>();
            if (handler) rowOf[handler.item] = (RectTransform)row;
        }

        int sib = 0;
        for (int i = 0; i < order.Count; i++)
        {
            int idx = order[i];
            if ((uint)idx >= (uint)itemsMap.Count) continue;

            if (rowOf.TryGetValue(itemsMap[idx], out var rt))
                rt.SetSiblingIndex(sib++);
        }
    }

    private static List<int> DefaultOrder(int n)
    {
        var list = new List<int>(n);
        for (int i = 0; i < n; i++) list.Add(i);
        return list;
    }
}
