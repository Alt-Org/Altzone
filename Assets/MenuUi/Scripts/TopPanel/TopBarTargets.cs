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

        private const bool DebugOn = false;

        private void OnEnable()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : OnEnable()");

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

            RectTransform parentRT;
            if (!IsValid(out parentRT)) return;

            bool[] vis = ReadVisibility();

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].visibilityTarget != null)
                    _rows[i].visibilityTarget.SetActive(vis[i]);
            }

            // List<int> rawOrder = SettingsCarrier.LoadTopBarOrderStatic(style, _rows.Count);
            //
            // List<int> orderedVisible = new List<int>(_rows.Count);
            //
            // for (int i = 0; i < rawOrder.Count; i++)
            // {
            //     int idx = rawOrder[i];
            //     if ((uint)idx < (uint)_rows.Count && vis[idx])
            //         orderedVisible.Add(idx);
            // }
            //
            // for (int i = 0; i < _rows.Count; i++)
            // {
            //     if (!vis[i]) continue;
            //     bool already = false;
            //     for (int j = 0; j < orderedVisible.Count; j++)
            //         if (orderedVisible[j] == i)
            //         {
            //             already = true;
            //             break;
            //         }
            //
            //     if (!already) orderedVisible.Add(i);
            // }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
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

            parentRT = null;

            if (_rows == null || _rows.Count == 0)
            {
                Debug.LogWarning("TopBarTargets: _rows is empty.");
                return false;
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].orderTarget != null)
                {
                    parentRT = _rows[i].orderTarget.transform.parent as RectTransform;
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

        // private static string PrefKeyForItem(TopBarDefs.TopBarItem item)
        // {
        //     if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : PrefKeyForItem()");
        //
        //     return TopBarDefs.Key(item);
        // }

        private bool[] ReadVisibility()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ReadVisibility()");

            int count = _rows.Count;
            bool[] vis = new bool[count];
            for (int i = 0; i < count; i++)
            {
                string key = TopBarDefs.Key(_rows[i].item) + "_" + style;
                vis[i] = PlayerPrefs.GetInt(key, 1) != 0;
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

            int sib = 0;
            HashSet<Transform> alreadyMoved = new HashSet<Transform>();

            for (int i = 0; i < orderedVisible.Count; i++)
            {
                Transform tr = _rows[orderedVisible[i]].orderTarget;

                if (tr == null) continue;

                if (tr.parent != parentRT) continue;

                if (alreadyMoved.Contains(tr)) continue;

                tr.transform.SetSiblingIndex(sib++);
                alreadyMoved.Add(tr);
            }

            EnsureSpacer(parentRT);
            _flexibleSpacer.gameObject.SetActive(true);
            _flexibleSpacer.SetSiblingIndex(sib);

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

        public void ApplyOrderFromSettings()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApllyOrderFromSettings()");

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

            List<int> rawOrder = SettingsCarrier.LoadTopBarOrderStatic(style, _rows.Count);

            List<int> orderedVisible = new List<int>(_rows.Count);

            for (int i = 0; i < rawOrder.Count; i++)
            {
                int idx = rawOrder[i];

                if ((uint)idx < (uint)_rows.Count && vis[idx])
                    orderedVisible.Add(idx);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (!vis[i]) continue;

                bool already = false;

                for (int j = 0; j < orderedVisible.Count; j++)
                {
                    if (orderedVisible[j] == i)
                    {
                        already = true;
                        break;
                    }
                }

                if (!already)
                    orderedVisible.Add(i);
            }

            ApplyOrderWithSpacer(parentRT, orderedVisible);
        }
    }
}
