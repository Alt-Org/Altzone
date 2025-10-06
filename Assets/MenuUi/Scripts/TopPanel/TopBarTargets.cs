using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Settings; // TopBarDefs

public class TopBarTargets : MonoBehaviour
{
    [System.Serializable]
    private class Row
    {
        public TopBarDefs.TopBarItem item;
        public GameObject target;
    }

    public SettingsCarrier.TopBarStyle style;

    [Header("Items (1:1)")]
    [SerializeField] private List<Row> _rows = new List<Row>();

    [Header("Spacer (created if null)")]
    [SerializeField] private RectTransform _flexibleSpacer;
    [SerializeField] private float _spacerMinWidth = 0f;

    [SerializeField] private GameObject _standaloneLeaderboard;   // erillinen LB-nappi
    [SerializeField] private GameObject _clanTileLeaderboard;     // LB-nappi clan-ruudussa

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
        RectTransform parentRT;
        if (!IsValid(out parentRT)) return;

        // 1) näkyvyys PlayerPrefsistä
        bool[] vis = ReadVisibility();

        // 2) Clan vs Leaderboard -sääntö
        bool clanOn, lbOn;
        ApplyClanLeaderboardRule(vis, out clanOn, out lbOn);

        // 3) Aktivoi/poista näkyvistä rivit
        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i].target != null)
                _rows[i].target.SetActive(vis[i]);
        }

        // LB-variantit
        if (_clanTileLeaderboard != null) _clanTileLeaderboard.SetActive(clanOn && lbOn);
        if (_standaloneLeaderboard != null) _standaloneLeaderboard.SetActive(lbOn && !clanOn);

        // 4) Järjestys SettingsCarrierista (List<int>) ja suodata näkyvät
        List<int> rawOrder = SettingsCarrier.LoadTopBarOrderStatic(style, _rows.Count);

        List<int> orderedVisible = new List<int>(_rows.Count);
        for (int i = 0; i < rawOrder.Count; i++)
        {
            int idx = rawOrder[i];
            if ((uint)idx < (uint)_rows.Count && vis[idx])
                orderedVisible.Add(idx);
        }
        // lisää puuttuvat näkyvät loppuun
        for (int i = 0; i < _rows.Count; i++)
        {
            if (!vis[i]) continue;
            bool already = false;
            for (int j = 0; j < orderedVisible.Count; j++)
                if (orderedVisible[j] == i) { already = true; break; }
            if (!already) orderedVisible.Add(i);
        }

        // 5) Aseta sisarusindeksit spacerilla
        ApplyOrderWithSpacer(parentRT, orderedVisible);
    }

    // ---- OrderBridge tarvitsee vain nämä kaksi apuria ----
    public int RowCount()
    {
        return _rows != null ? _rows.Count : 0;
    }

    public TopBarDefs.TopBarItem GetItemAt(int index)
    {
        if (_rows == null || index < 0 || index >= _rows.Count) return default(TopBarDefs.TopBarItem);
        return _rows[index].item;
    }
    // ------------------------------------------------------

    private bool IsValid(out RectTransform parentRT)
    {
        parentRT = null;

        if (_rows == null || _rows.Count == 0)
        {
            Debug.LogWarning("TopBarTargets: _rows is empty.");
            return false;
        }

        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i].target != null)
            {
                parentRT = _rows[i].target.transform.parent as RectTransform;
                if (parentRT != null) break;
            }
        }

        if (parentRT == null)
        {
            Debug.LogWarning("TopBarTargets: parent RectTransform not found.");
            return false;
        }
        return true;
    }

    private static string PrefKeyForItem(TopBarDefs.TopBarItem item)
    {
        return TopBarDefs.Key(item);
    }

    private bool[] ReadVisibility()
    {
        int count = _rows.Count;
        bool[] vis = new bool[count];
        for (int i = 0; i < count; i++)
        {
            string key = TopBarDefs.Key(_rows[i].item) + "_" + style; // tyylikohtainen
            vis[i] = PlayerPrefs.GetInt(key, 1) != 0; // default ON
        }
        return vis;
    }


    private void ApplyClanLeaderboardRule(bool[] vis, out bool clanOn, out bool lbOn)
    {
        int clanIdx = IndexOfItem(_clanItem);
        int lbIdx = IndexOfItem(_leaderboardItem);

        clanOn = clanIdx >= 0 && vis[clanIdx];
        lbOn = lbIdx >= 0 && vis[lbIdx];

        // molemmat päällä -> piilota standalone LB -rivi
        if (clanOn && lbOn && lbIdx >= 0)
            vis[lbIdx] = false;
    }

    private int IndexOfItem(TopBarDefs.TopBarItem item)
    {
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i].item.Equals(item)) return i;
        return -1;
    }

    private void EnsureSpacer(RectTransform parent)
    {
        if (_flexibleSpacer == null)
        {
            GameObject go = new GameObject("FlexibleSpacer", typeof(RectTransform), typeof(LayoutElement));
            _flexibleSpacer = go.GetComponent<RectTransform>();

            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
            cg.alpha = 0f;
        }

        if (_flexibleSpacer.parent != parent)
            _flexibleSpacer.SetParent(parent, false);

        LayoutElement le = _flexibleSpacer.GetComponent<LayoutElement>();
        le.minWidth = _spacerMinWidth;
        le.preferredWidth = 0f;
        le.flexibleWidth = 1f; // työntää viimeisen oikealle
        le.minHeight = 0f;
        le.preferredHeight = 0f;
        le.flexibleHeight = 0f;
    }

    private void ApplyOrderWithSpacer(RectTransform parentRT, List<int> orderedVisible)
    {
        int n = orderedVisible.Count;

        if (n <= 1)
        {
            if (n == 1 && _rows[orderedVisible[0]].target != null)
                _rows[orderedVisible[0]].target.transform.SetSiblingIndex(0);

            if (_flexibleSpacer != null) _flexibleSpacer.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
            return;
        }

        int sib = 0;
        for (int i = 0; i < n - 1; i++)
        {
            GameObject go = _rows[orderedVisible[i]].target;
            if (go != null) go.transform.SetSiblingIndex(sib++);
        }

        EnsureSpacer(parentRT);
        _flexibleSpacer.gameObject.SetActive(true);
        _flexibleSpacer.SetSiblingIndex(sib++);

        GameObject last = _rows[orderedVisible[n - 1]].target;
        if (last != null) last.transform.SetSiblingIndex(sib);

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
    }

    // ... muu TopBarTargets kuten sinulla jo on ...

    public bool TryGetRowIndex(TopBarDefs.TopBarItem item, out int index)
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            if (_rows[i].item.Equals(item))
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }



}
