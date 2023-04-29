using System.Collections.Generic;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.ChangeRegion
{
    /// <summary>
    /// <c>View</c> for ChangeRegion UI to show all Photon enabled regions and select one of them.
    /// </summary>
    public class ChangeRegionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameObject _lineTemplate;
        [SerializeField] private Transform _contentRoot;

        private List<LineData> _lineDataList = new();

        public string TitleText
        {
            get => _titleText.text;
            set => _titleText.text = value;
        }

        private void Awake()
        {
            // Hide line template if it is active in Editor.
            _lineTemplate.SetActive(false);
        }

        public void ResetView()
        {
            TitleText = string.Empty;
            DeleteExtraLines(_contentRoot, 0, ref _lineDataList);
        }

        public void UpdateRegionList(IReadOnlyList<PhotonRegionList.PhotonRegion> regions)
        {
            var regionsCount = regions.Count;
            if (_contentRoot.childCount > regionsCount)
            {
                DeleteExtraLines(_contentRoot, regionsCount, ref _lineDataList);
            }
            else if (_contentRoot.childCount < regionsCount)
            {
                AddMoreTextLines(_contentRoot, regionsCount, _lineTemplate, ref _lineDataList);
            }
            for (var i = 0; i < _contentRoot.childCount; ++i)
            {
                _lineDataList[i].UpdateRegion(regions[i]);
            }
        }

        private static void DeleteExtraLines(Transform parent, int minCount, ref List<LineData> lineDataList)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= minCount; --i)
            {
                var child = parent.GetChild(i).gameObject;
                var index = lineDataList.FindIndex(x => x._parent.Equals(child));
                if (index >= 0)
                {
                    lineDataList.RemoveAt(index);
                }
                Destroy(child);
            }
        }

        private static void AddMoreTextLines(Transform parent, int maxCount, GameObject template, ref List<LineData> lineDataList)
        {
            while (parent.childCount < maxCount)
            {
                var instance = Instantiate(template, parent);
                instance.SetActive(true);
                lineDataList.Add(new LineData(instance));
            }
        }

        private static string GetRegionName(string regionCode)
        {
            if (RegionNames.TryGetValue(regionCode, out var regionName))
            {
                return regionName;
            }
            return regionCode.ToUpper();
        }

        private static Dictionary<string, string> RegionNames = new()
        {
            // https://doc.photonengine.com/realtime/current/connection-and-authentication/regions#available_regions
            { "asia", "Asia, Singapore" },
            { "au", "Australia, Melbourne" },
            { "cae", "Canada, East, Montreal" },
            { "cn", "Chinese Mainland, Shanghai" },
            { "eu", "Europe, Amsterdam" },
            { "in", "India, Chennai" },
            { "jp", "Japan, Tokyo" },
            { "za", "South Africa, Johannesburg" },
            { "sa", "South America, Sao Paulo" },
            { "kr", "South Korea, Seoul" },
            { "tr", "Turkey, Istanbul" },
            { "us", "USA, East, Washington D.C." },
            { "usw", "USA, West, San José" },
        };

        private class LineData
        {
            public readonly GameObject _parent;
            private readonly TextMeshProUGUI _code;
            private readonly TextMeshProUGUI _name;
            private readonly TextMeshProUGUI _ping;

            private PhotonRegionList.PhotonRegion _region;

            public LineData(GameObject parent)
            {
                _parent = parent;
                var textComponents = parent.GetComponentsInChildren<TextMeshProUGUI>();
                _code = textComponents[0];
                _name = textComponents[1];
                _ping = textComponents[2];
                var button = parent.GetComponentsInChildren<Button>()[0];
                button.onClick.AddListener(() =>
                {
                    if (_parent == null)
                    {
                        // We have been destroyed!
                        return;
                    }
                    Debug.Log($"click {_region?.Region}");
                });
            }

            public void UpdateRegion(PhotonRegionList.PhotonRegion region)
            {
                _region = region;
                _code.text = region.Region;
                _name.text = GetRegionName(region.Region);
                _ping.text = region.Ping > 0 ? $"{region.Ping} ms" : "---   ";
            }
        }
    }
}