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
    [SerializeField] private TopBarClanTileLayout[] _topBarToggleLayouts;
    [SerializeField] private TopBarToggleHandler[] _topBarToggleHandlers;
    [SerializeField] private GameObject _notInUsePanel;

    public static TopBarOrderBridge Active { get; private set; }
    private const bool DebugOn = true;

    private SettingsCarrier.TopBarStyle CurrentStyle =>
        SettingsCarrier.Instance
            ? SettingsCarrier.Instance.TopBarStyleSetting
            : SettingsCarrier.TopBarStyle.NewHelena;

    public TopBarTargets[] TargetsByStyle { get => _targetsByStyle; }

    private void Awake()
    {
        Debug.Log("[TopBarDebug] TopBarOrderBridge : Awake()");
    }

    private void OnEnable()
    {
        Active = this;

        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : OnEnable()");

        if (_targetsByStyle == null || _targetsByStyle.Length == 0)
        {
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

        Debug.Log($"[TopBarDebug] CurrentStyle={CurrentStyle}, " +
                  $"owner={(owner ? owner.name : "NULL")}, " +
                  $"toggleContainer={(_toggleContainer ? _toggleContainer.name : "NULL")}");

        if (owner == null || _toggleContainer == null) return;

        Dictionary<int, TopBarDefs.TopBarItem> order =
            new Dictionary<int, TopBarDefs.TopBarItem>(_toggleContainer.childCount);
        int nextPos = 0;

        bool clanTileOn = PlayerPrefs.GetInt(
            TopBarDefs.Key(TopBarDefs.TopBarItem.Tile) + "_" + CurrentStyle,
            1
        ) != 0;

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
            TopBarDefs.TopBarItem item = owner.GetItemAt(i);
            if (!order.ContainsValue(item))
            {
                order[nextPos] = item;
                nextPos++;
            }
        }

        List<int> positions = new List<int>(order.Keys);
        positions.Sort();

        List<int> indices = new List<int>(positions.Count);
        foreach (int pos in positions)
        {
            if (!order.TryGetValue(pos, out TopBarDefs.TopBarItem item)) continue;

            int index = -1;
            for (int i = 0; i < total; i++)
            {
                if (owner.GetItemAt(i).Equals(item))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0 && !indices.Contains(index)) indices.Add(index);
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
                TopBarDefs.Key(TopBarDefs.TopBarItem.Tile) + "_" + CurrentStyle,
                1
            ) != 0
        );
    }

    private void UpdateTopBarStyle(SettingsCarrier.TopBarStyle style)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : UpdateTopBarStyle()");

        TopBarTargets owner = GetTargetsFor(style);

        if (owner != null)
            Debug.Log($"[TopBarDebug] Bridge target = {owner.name}, style={owner.style}, rows={owner.RowCount()}");

        if (owner == null || _toggleContainer == null) return;

        int total = owner.RowCount();

        List<int> orderList = SettingsCarrier.LoadTopBarOrderStatic(style, total);

        Dictionary<int, TopBarDefs.TopBarItem> order = new(orderList.Count);
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
            TopBarDefs.TopBarItem item = owner.GetItemAt(i);
            if (!order.ContainsValue(item))
            {
                order[pos] = item;
                pos++;
            }
        }
        //Old Theme is disabled for now...
        if (CurrentStyle != SettingsCarrier.TopBarStyle.Old)
        {
            owner.ApplyFromSettings();
            owner.ApplyOrderFromSettings();
            ApplyOrderToToggleList(order, _toggleContainer, owner);
        }

        bool clanTileOn = PlayerPrefs.GetInt(
            TopBarDefs.Key(TopBarDefs.TopBarItem.Tile) + "_" + style,
            1
        ) != 0;

        SetClanSubItemIndent(clanTileOn);
        ToggleUpdate();
    }

    private static void ApplyOrderToToggleList(
        Dictionary<int, TopBarDefs.TopBarItem> order,
        RectTransform container,
        TopBarTargets owner)
    {
        if (DebugOn) Debug.Log($"[TopBarDebug] TopBarOrderBridge : ApplyOrderToToggleList()");

        Dictionary<TopBarDefs.TopBarItem, RectTransform> rowOf = new(container.childCount);

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
            if (!order.TryGetValue(pos, out TopBarDefs.TopBarItem item)) continue;

            if (rowOf.TryGetValue(item, out RectTransform rt))
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

        //if (clanTileOn)
            //MoveClanSubRowsUnderClanTile();
    }

    public void RefreshClanSubItemIndent()
    {
        SetClanSubItemIndent(
            PlayerPrefs.GetInt(
                TopBarDefs.Key(TopBarDefs.TopBarItem.Tile) + "_" + CurrentStyle,
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

    //Checks what toggles are on this theme
    private void ToggleUpdate()
    {
        //Old Theme is disabled for now...
        if(CurrentStyle == SettingsCarrier.TopBarStyle.Old)
        {
            _notInUsePanel.SetActive(true);
            return;
        }
        _notInUsePanel.SetActive(false);

        foreach (var t in TargetsByStyle)
        {
            //Checks what theme is on
            if (!t.gameObject.activeSelf)
                continue;

            //Releases all the toggles from clantile toggles
            foreach (var i in _topBarToggleLayouts)
            {
                i.SetTogglesFree();
            }
            //Sets new toggles to the clantile depending what objects are on clan tile
            foreach (var i in _topBarToggleLayouts)
            {
                StartCoroutine(i.IsThereATile(t));
            }

            //Checks what toggless are on this theme
            foreach (var i in _topBarToggleHandlers)
            {
                foreach (var e in t.Rows)
                {
                    if (i.item == e.item)
                    {
                        i.gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        i.gameObject.SetActive(false);
                    }
                }
            }
        }

    }

    private void MoveClanSubRowsUnderClanTile()
    {
        if (_toggleContainer == null) return;

        RectTransform clanTileRow = null;
        RectTransform leaderboardRow = null;
        RectTransform coinsRow = null;
        RectTransform clanLogoRow = null;
        RectTransform clanTextRow = null;

        foreach (Transform child in _toggleContainer)
        {
            TopBarToggleHandler h = child.GetComponentInChildren<TopBarToggleHandler>(true);
            if (h == null) continue;

            RectTransform row = child as RectTransform;

            switch (h.item)
            {
                case TopBarDefs.TopBarItem.Tile:
                    clanTileRow = row;
                    break;
                case TopBarDefs.TopBarItem.Leaderboard:
                    leaderboardRow = row;
                    break;
                case TopBarDefs.TopBarItem.Coins:
                    coinsRow = row;
                    break;
                case TopBarDefs.TopBarItem.ClanLogo:
                    clanLogoRow = row;
                    break;
                case TopBarDefs.TopBarItem.ClanTextContainer:
                    clanTextRow = row;
                    break;
            }
        }

        if (clanTileRow == null) return;

        int index = clanTileRow.GetSiblingIndex() + 1;

        if (leaderboardRow != null) leaderboardRow.SetSiblingIndex(index++);
        if (coinsRow != null) coinsRow.SetSiblingIndex(index++);
        if (clanLogoRow != null) clanLogoRow.SetSiblingIndex(index++);
        if (clanTextRow != null) clanTextRow.SetSiblingIndex(index++);
    }
}
