using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Settings;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.TopPanel
{
    public class TopBarTargets : MonoBehaviour
    {
        [System.Serializable]
        public class Row
        {
            public TopBarDefs.TopBarItem item;
            public GameObject visibilityTarget;
            public Transform orderTarget;
        }

        public SettingsCarrier.TopBarStyle style;

        [Header("Items (1:1)")] [SerializeField]
        private List<Row> _rows = new List<Row>();
        public List<Row> Rows { get => _rows; }
        [Header("Spacer (created if null)")] [SerializeField]
        private GameObject _flexibleSpacerPrefab;

        private List<GameObject> _spacerList = new();

        [SerializeField] private float _spacerMinWidth = 0f;

        [SerializeField] private TopBarDefs.TopBarItem _tileItem1st = TopBarDefs.TopBarItem.Tile;
        [SerializeField] private TopBarDefs.TopBarItem _leaderboardItem = TopBarDefs.TopBarItem.Leaderboard;
        [SerializeField] private TopBarDefs.TopBarItem _tileItem2nd = TopBarDefs.TopBarItem.Tile2nd;

        [SerializeField] private Transform _topBarContent;

        //SerializeField for slots in tiles panels
        [SerializeField] private GameObject _dropdownButton;

        [Serializable]
        public class TileManagement
        {
            public TopBarDefs.TopBarItem Tile;
            public Transform TilePanelRoot;
            public Transform TilePanel;
            public List<TileObjects> TileObjects;
        }

        [SerializeField] private List<TileManagement> _tileManagement;

        public List<TileManagement> PTileManagement { get => _tileManagement; }

        private const bool DebugOn = true;

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

            Debug.Log($"[TB] APPLY style={style} " +
                      $"active={gameObject.activeInHierarchy} " +
                      $"name={name}");

            if (!IsValid(out RectTransform parentRT)) return;

            bool[] isVisible = ReadVisibility();

            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].visibilityTarget != null)
                    _rows[i].visibilityTarget.SetActive(isVisible[i]);
            }

            ApplyClanPanelMode(TopBarDefs.TopBarItem.Tile);
            ApplyClanPanelMode(TopBarDefs.TopBarItem.Tile2nd);

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

            int clanIdx = IndexOfItem(_tileItem1st);
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

        private GameObject EnsureSpacer(RectTransform parent)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : EnsureSpacer()");

            if (_flexibleSpacerPrefab == null)
            {
                GameObject go = new GameObject("FlexibleSpacer", typeof(RectTransform), typeof(LayoutElement));
                _flexibleSpacerPrefab = go;

                LayoutElement le = _flexibleSpacerPrefab.GetComponent<LayoutElement>();
                le.minWidth = _spacerMinWidth;
                le.preferredWidth = 0f;
                le.flexibleWidth = 1000f;
                le.minHeight = 0f;
                le.preferredHeight = 0f;
                le.flexibleHeight = 0f;
            }

            GameObject spacer = Instantiate(_flexibleSpacerPrefab, parent);

            if (spacer.transform.parent != parent)
                spacer.transform.SetParent(parent, false);

            return spacer;
        }

        private void ApplyOrderWithSpacer(RectTransform parentRT, List<int> orderedVisible)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyOrderWithSpacer()");

            bool clanPanelOn = false;
            TileManagement clanTileRow = null;
            bool playerPanelOn = false;
            TileManagement playerTileRow = null;

            List<Row> visibleRows = _rows.Where(x => x.visibilityTarget.activeSelf).ToList();

            foreach (Row i in _rows)
            {
                if(i.item is TopBarDefs.TopBarItem.Tile)
                {
                    clanPanelOn = i.visibilityTarget.activeSelf;
                    clanTileRow = _tileManagement.FirstOrDefault(x => x.Tile is TopBarDefs.TopBarItem.Tile);
                }
                if (i.item is TopBarDefs.TopBarItem.Tile2nd)
                {
                    playerPanelOn = i.visibilityTarget.activeSelf;
                    playerTileRow = _tileManagement.FirstOrDefault(x => x.Tile is TopBarDefs.TopBarItem.Tile2nd);
                }
            }

            int sib = 0;

            HashSet<Transform> alreadyMoved = new HashSet<Transform>();
            List<Row> spaceredRows = visibleRows;

            foreach (int rowIndex in orderedVisible)
            {
                TopBarDefs.TopBarItem item = _rows[rowIndex].item;

                if (clanPanelOn)
                {
                    bool isClanSubItem = false;
                    foreach (var e in clanTileRow.TileObjects)
                    {
                        if (item == e.Tag)
                        {
                            isClanSubItem = true;
                            spaceredRows.Remove(_rows[rowIndex]);
                            break;
                        }
                    }
                    if (isClanSubItem)
                        continue;
                }

                if (playerPanelOn)
                {
                    bool isClanSubItemF2nd = false;
                    foreach (var e in playerTileRow.TileObjects)
                    {
                        if (item == e.Tag)
                        {
                            isClanSubItemF2nd = true;
                            spaceredRows.Remove(_rows[rowIndex]);
                            break;
                        }
                    }
                    if (isClanSubItemF2nd)
                        continue;
                }

                Transform tr = _rows[rowIndex].orderTarget;
                if (tr == null) continue;

                if (tr.parent != parentRT)
                    tr.SetParent(parentRT, false);

                if (alreadyMoved.Contains(tr)) continue;

                tr.SetSiblingIndex(sib++);
                alreadyMoved.Add(tr);
            }

            //Spacer System
            if (_spacerList.Count == 0)
            {
                for(int i = 0; i < _rows.Count - 1 ; i++)
                {
                    GameObject spacer = EnsureSpacer(parentRT);
                    _spacerList.Add(spacer);
                }
            }

            for (int i = 0; i < _spacerList.Count; i++)
            {
                if(i < spaceredRows.Count)
                {
                    _spacerList[i].SetActive(true);
                    _spacerList[i].transform.SetSiblingIndex(i*2+1);
                }
                else
                {
                    _spacerList[i].SetActive(false);
                    _spacerList[i].transform.SetAsLastSibling();
                }
            }
            int sectionCount = 1;
            foreach (Row row in spaceredRows)
            {
                if (row.item is TopBarDefs.TopBarItem.Tile or TopBarDefs.TopBarItem.Tile2nd) sectionCount += 3;
                else sectionCount++;
            }
            float topbarWidth = parentRT.rect.width;
            float sectionWidth = Mathf.Min((topbarWidth - 10 * spaceredRows.Count*2) /sectionCount, parentRT.rect.height);

            foreach (Row row in spaceredRows)
            {
                if (row.item is TopBarDefs.TopBarItem.Tile or TopBarDefs.TopBarItem.Tile2nd) row.visibilityTarget.GetComponent<LayoutElement>().minWidth = sectionWidth*3;
                else row.visibilityTarget.GetComponent<LayoutElement>().minWidth = sectionWidth;
            }
            _dropdownButton.GetComponent<LayoutElement>().minWidth = sectionWidth;

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
                   _tileManagement.All(x => x != null);
        }

        public void ApplyOrderFromSettings()
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : ApplyOrderFromSettings()");

            if (!IsValid(out RectTransform parentRT)) return;

            if (DebugOn)
            {
                for (int i = 0; i < parentRT.childCount; i++)
                {
                    Transform child = parentRT.GetChild(i);
                    Debug.Log($"[{i}] {child.name} (active: {child.gameObject.activeSelf})");
                }
            }

            bool[] visibleRows = ReadVisibility();

            List<int> rawOrder = SettingsCarrier.LoadTopBarOrderStatic(style, _rows.Count);

            Debug.Log("[TopBarDebugOn] RAW ORDER:");
            foreach (int idx in rawOrder)
            {
                Debug.Log($"[TopBarDebugOn] : idx={idx}, item={_rows[idx].item}, visible={visibleRows[idx]}");
            }

            List<int> ordered = new(_rows.Count);

            foreach (int index in rawOrder)
            {
                if ((uint)index >= (uint)_rows.Count) continue;

                if (!visibleRows[index])
                    continue;

                if (!ordered.Contains(index))
                    ordered.Add(index);
            }

            ApplyOrderWithSpacer(parentRT, ordered);
        }

        private void ApplyClanPanelMode(TopBarDefs.TopBarItem Tags)
        {
            bool clanPanelOn = false;
            List<Row> visibleRows = _rows.Where(x => x.visibilityTarget.activeSelf).ToList();
            TileManagement clanTileRow = null;

            foreach (Row i in visibleRows)
            {
                if (i.item == Tags)
                {
                    clanPanelOn = i.visibilityTarget.activeSelf;
                    clanTileRow = _tileManagement.FirstOrDefault(x => x.Tile == Tags);
                }
            }

            if (DebugOn)
                Debug.Log($"[TB] ApplyClanPanelMode clanPanelOn={clanPanelOn}");

            if (clanTileRow == null) return;

            if (clanPanelOn)
            {
                if (clanTileRow.TilePanelRoot == null)
                    return;

                clanTileRow.TilePanelRoot.gameObject.SetActive(true);
                    
                foreach (var objects in clanTileRow.TileObjects)
                {
                    MoveToSlot(objects.Child, objects.SlotContainer);
                }
            }
            else
            {

                if (clanTileRow.TilePanelRoot == null)
                    return;

                clanTileRow.TilePanelRoot.gameObject.SetActive(false);

                foreach (var objects in clanTileRow.TileObjects)
                {
                    MoveToTopBar(objects.Child, _topBarContent);
                }
            }

        }

        private void MoveUnderClanPanel(Transform item)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : MoveUnderClanPanel()");

            //if (item == null || _clanPanel == null) return;
            //item.SetParent(_clanPanel, false);
        }

        private void MoveToTopBar(Transform item, Transform parent)
        {
            if (DebugOn) Debug.Log($"[TopBarDebug] TopBarTargets : MoveToTopBar()");

            if (item == null || parent == null) return;

            item.SetParent(parent, false);
            item.gameObject.SetActive(true);
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

        [Serializable]
        public class TileObjects
        {
            public TopBarDefs.TopBarItem Tag;
            public Transform Child;
            public Transform SlotContainer;
        }
    }
}
