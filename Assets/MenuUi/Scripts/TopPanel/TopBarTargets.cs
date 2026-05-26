using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Settings;

namespace MenuUI.Scripts.TopPanel
{
    public class TopBarTargets : MonoBehaviour
    {
        [System.Serializable]
        private class Row
        {
            public TopBarDefs.TopBarItem item;
            public GameObject visibilityTarget;
            public Transform orderTarget;
        }

        public SettingsCarrier.TopBarStyle style;

        [Header("Items (1:1)")] [SerializeField]
        private List<Row> _rows = new List<Row>();

        [Header("Spacer (created if null)")] [SerializeField]
        private RectTransform _flexibleSpacer;

        [SerializeField] private float _spacerMinWidth = 0f;

        [SerializeField] private GameObject _standaloneLeaderboard;
        [SerializeField] private GameObject _clanTileLeaderboard;
        [SerializeField] private TopBarDefs.TopBarItem _clanItem = TopBarDefs.TopBarItem.ClanTile;
        [SerializeField] private TopBarDefs.TopBarItem _leaderboardItem = TopBarDefs.TopBarItem.Leaderboard;

        [SerializeField] private Transform _topBarContent;
        [SerializeField] private Transform _clanPanelRoot;
        [SerializeField] private Transform _clanPanel;

        [SerializeField] private Transform _clanLeaderboardButton;
        [SerializeField] private Transform _clanHeart;
        [SerializeField] private Transform _textContainer;
        [SerializeField] private Transform _coinsRow;

        //SerializeField for slots in ClanPanel
        [SerializeField] private Transform _slotLeaderboard;
        [SerializeField] private Transform _slotCoins;
        [SerializeField] private Transform _slotClanLogo;
        [SerializeField] private Transform _slotTextContainer;

        private const bool DebugOn = true;

        private void OnEnable()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : OnEnable()");
            //
            // PlayerPrefs.DeleteAll();
            // PlayerPrefs.Save();

            ApplyFromSettings();
            SettingsCarrier.OnTopBarChanged += OnCarrierChanged;
        }

        private void OnDisable()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : OnDisable()");

            SettingsCarrier.OnTopBarChanged -= OnCarrierChanged;
        }

        private void OnCarrierChanged(int styleIndex)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : OnCarrieChanged()");

            if ((SettingsCarrier.TopBarStyle)styleIndex == style)
                ApplyFromSettings();
        }

        public void ApplyFromSettings()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyFromSettings()");

            Debug.Log($"[TB] APPLY style={style} " +
                      $"active={gameObject.activeInHierarchy} " +
                      $"name={name}");

            RectTransform parentRT;
            if (!IsValid(out parentRT)) return;

            bool[] isVisible = ReadVisibility();

            bool clanPanelOn = IsVisible(TopBarDefs.TopBarItem.ClanTile);

            Debug.Log($"[TB] BEFORE clanOn={clanPanelOn} " +
                      $"heartParent={_clanHeart.parent.name}, " +
                      $"textParent={_textContainer.parent.name}, " +
                      $"coinsParent={_coinsRow.parent.name}");

            ApplyClanPanelMode(clanPanelOn);

            Debug.Log($"[TB] AFTER clanOn={clanPanelOn} " +
                      $"heartParent={_clanHeart.parent.name}, " +
                      $"textParent={_textContainer.parent.name}, " +
                      $"coinsParent={_coinsRow.parent.name}");

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].visibilityTarget != null)
                    _rows[i].visibilityTarget.SetActive(isVisible[i]);
            }

            ApplyOrderFromSettings();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);

            StartCoroutine(CheckAfterFrame());
        }


        public int RowCount()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : RowCount()");

            return _rows != null ? _rows.Count : 0;
        }

        public TopBarDefs.TopBarItem GetItemAt(int index)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : GetItemAt()");

            if (_rows == null || index < 0 || index >= _rows.Count) return default(TopBarDefs.TopBarItem);
            return _rows[index].item;
        }


        private bool IsValid(out RectTransform parentRT)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : IsValid()");

            parentRT = _topBarContent as RectTransform;

            Debug.Log($"[TB] IsValid target={gameObject.name}, " +
                      $"style={style}, " +
                      $"topBarContent={_topBarContent?.name}");

            if (_rows == null || _rows.Count == 0)
            {
                Debug.LogWarning($"[TB] INVALID: rows empty on {gameObject.name}");
                return false;
            }

            if (parentRT == null)
            {
                Debug.LogWarning($"[TB] INVALID: _topBarContent missing on {gameObject.name}");
                return false;
            }

            return true;
        }

        private static string PrefKeyForItem(TopBarDefs.TopBarItem item)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : PrefKeyForItem()");

            return TopBarDefs.Key(item);
        }

        private bool[] ReadVisibility()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ReadVisibility()");

            int count = _rows.Count;
            bool[] vis = new bool[count];
            for (int i = 0; i < count; i++)
            {
                string key = TopBarDefs.Key(_rows[i].item) + "_" + style;
                vis[i] = PlayerPrefs.GetInt(key, 1) != 0;

                Debug.Log($"[TB] READ row={i} " +
                          $"item={_rows[i].item} " +
                          $"style={style} key={key} " +
                          $"value={vis[i]}");
            }

            return vis;
        }


        private void ApplyClanLeaderboardRule(bool[] vis, out bool clanOn, out bool lbOn)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyClanLeaderboardRule()");

            int clanIdx = IndexOfItem(_clanItem);
            int lbIdx = IndexOfItem(_leaderboardItem);

            clanOn = clanIdx >= 0 && vis[clanIdx];
            lbOn = lbIdx >= 0 && vis[lbIdx];

            if (clanOn && lbOn && lbIdx >= 0)
                vis[lbIdx] = false;
        }

        private int IndexOfItem(TopBarDefs.TopBarItem item)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : IndexOfItem()");

            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i].item.Equals(item))
                    return i;
            return -1;
        }

        private void EnsureSpacer(RectTransform parent)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : EnsureSpacer()");

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
            le.flexibleWidth = 1f;
            le.minHeight = 0f;
            le.preferredHeight = 0f;
            le.flexibleHeight = 0f;
        }

        private void ApplyOrderWithSpacer(RectTransform parentRT, List<int> orderedVisible)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyOrderWithSpacer()");

            bool clanPanelOn = IsVisible(TopBarDefs.TopBarItem.ClanTile);

            int sib = 0;
            HashSet<Transform> alreadyMoved = new HashSet<Transform>();

            for (int i = 0; i < orderedVisible.Count; i++)
            {
                int rowIndex = orderedVisible[i];
                TopBarDefs.TopBarItem item = _rows[rowIndex].item;

                bool isClanSubItem =
                    item == TopBarDefs.TopBarItem.Leaderboard ||
                    item == TopBarDefs.TopBarItem.ClanLogo ||
                    item == TopBarDefs.TopBarItem.ClanTextContainer ||
                    item == TopBarDefs.TopBarItem.Coins;

                if (clanPanelOn && isClanSubItem)
                    continue;

                Transform tr = _rows[rowIndex].orderTarget;
                if (tr == null) continue;

                if (tr.parent != parentRT)
                    tr.SetParent(parentRT, false);

                if (alreadyMoved.Contains(tr)) continue;

                tr.SetSiblingIndex(sib++);
                alreadyMoved.Add(tr);
            }

            EnsureSpacer(parentRT);
            _flexibleSpacer.gameObject.SetActive(true);
            _flexibleSpacer.SetSiblingIndex(sib);

            Debug.Log("[TB] FINAL TOPBAR CHILDREN:");
            for (int i = 0; i < parentRT.childCount; i++)
            {
                Transform c = parentRT.GetChild(i);
                Debug.Log($"[TB] {i}: {c.name}, " +
                          $"active={c.gameObject.activeSelf}, " +
                          $"parent={c.parent.name}");
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
        }

        public bool TryGetRowIndex(TopBarDefs.TopBarItem item, out int index)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : TryGetRowIndex()");

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

        public bool IsReady()
        {
            return _topBarContent != null &&
                   _clanPanelRoot != null &&
                   _clanPanel != null &&
                   _clanLeaderboardButton != null &&
                   _clanHeart != null &&
                   _textContainer != null &&
                   _coinsRow != null;
        }

        public void ApplyOrderFromSettings()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyOrderFromSettings()");

            RectTransform parentRT;
            if (!IsValid(out parentRT)) return;

            if (DebugOn)
            {
                for (int i = 0; i < parentRT.childCount; i++)
                {
                    Transform child = parentRT.GetChild(i);
                    Debug.Log($"[{i}] {child.name} (active: {child.gameObject.activeSelf})");
                }
            }

            bool[] vis = ReadVisibility();
            bool clanPanelOn = IsVisible(TopBarDefs.TopBarItem.ClanTile);

            List<int> rawOrder = SettingsCarrier.LoadTopBarOrderStatic(style, _rows.Count);

            Debug.Log("[TopBarDebugOn] RAW ORDER:");
            foreach (int idx in rawOrder)
            {
                Debug.Log($"[TopBarDebugOn] : idx={idx}, item={_rows[idx].item}, visible={vis[idx]}");
            }

            List<int> ordered = new List<int>(_rows.Count);

            foreach (int idx in rawOrder)
            {
                if ((uint)idx >= (uint)_rows.Count) continue;
                //if (!vis[idx]) continue;

                TopBarDefs.TopBarItem item = _rows[idx].item;

                bool isClanSubItem =
                    item == TopBarDefs.TopBarItem.Leaderboard ||
                    item == TopBarDefs.TopBarItem.ClanLogo ||
                    item == TopBarDefs.TopBarItem.ClanTextContainer ||
                    item == TopBarDefs.TopBarItem.Coins;

                // if (clanPanelOn && isClanSubItem)
                //     continue;

                // if (!clanPanelOn && item == TopBarDefs.TopBarItem.ClanTile)
                // {
                //     AddIfVisible(TopBarDefs.TopBarItem.Leaderboard, vis, ordered);
                //     AddIfVisible(TopBarDefs.TopBarItem.ClanLogo, vis, ordered);
                //     AddIfVisible(TopBarDefs.TopBarItem.ClanTextContainer, vis, ordered);
                //     AddIfVisible(TopBarDefs.TopBarItem.Coins, vis, ordered);
                //     continue;
                // }

                if (!vis[idx])
                    continue;

                if (!ordered.Contains(idx))
                    ordered.Add(idx);
            }

            ApplyOrderWithSpacer(parentRT, ordered);
        }

        private void ApplyClanPanelMode(bool clanPanelOn)
        {
            if (DebugOn)
                Debug.Log($"[TB] ApplyClanPanelMode clanPanelOn={clanPanelOn}");

            if (clanPanelOn)
            {
                if (_clanPanelRoot != null)
                    _clanPanelRoot.gameObject.SetActive(true);

                MoveToSlot(_clanLeaderboardButton, _slotLeaderboard);
                MoveToSlot(_coinsRow, _slotCoins);
                MoveToSlot(_clanHeart, _slotClanLogo);
                MoveToSlot(_textContainer, _slotTextContainer);
            }
            else
            {
                if (_clanPanelRoot != null)
                    _clanPanelRoot.gameObject.SetActive(false);

                MoveToTopBar(_clanLeaderboardButton, _topBarContent);
                MoveToTopBar(_coinsRow, _topBarContent);
                MoveToTopBar(_clanHeart, _topBarContent);
                MoveToTopBar(_textContainer, _topBarContent);
            }
        }

        private void MoveUnderClanPanel(Transform item)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : MoveUnderClanPanel()");

            if (item == null || _clanPanel == null) return;
            item.SetParent(_clanPanel, false);
        }

        private void MoveToTopBar(Transform item, Transform parent)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : MoveToTopBar()");

            if (item == null || parent == null) return;

            item.SetParent(parent, false);
            item.SetParent(parent, false);
            item.gameObject.SetActive(true);
        }

        private bool IsVisible(TopBarDefs.TopBarItem item)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : IsVisible()");

            string key = TopBarDefs.Key(item) + "_" + style;
            return PlayerPrefs.GetInt(key, 1) != 0;
        }

        private void AddIfVisible(TopBarDefs.TopBarItem item, bool[] vis, List<int> ordered)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : AddIfVisible()");

            int idx = IndexOfItem(item);

            if (idx < 0)
                return;

            if (!vis[idx])
                return;

            if (!ordered.Contains(idx))
                ordered.Add(idx);
        }

        private System.Collections.IEnumerator CheckAfterFrame()
        {
            yield return null;

            Debug.Log("[TB AFTER FRAME]");
            foreach (var row in _rows)
            {
                if (row.visibilityTarget == null) continue;

                Debug.Log($"[TB AFTER FRAME] item={row.item}, " +
                          $"active={row.visibilityTarget.activeSelf}, " +
                          $"target={row.visibilityTarget.name}, " +
                          $"parent={row.visibilityTarget.transform.parent.name}");
            }
        }

        private void MoveToSlot(Transform item, Transform slot)
        {
            if (item == null || slot == null)
                return;

            item.SetParent(slot, false);
            item.SetSiblingIndex(0);
            item.gameObject.SetActive(true);

            if (item is RectTransform rt)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
        }
    }
}
