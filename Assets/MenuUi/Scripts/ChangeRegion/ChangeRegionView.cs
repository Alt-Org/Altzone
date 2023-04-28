using System.Collections.Generic;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;

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
            DeleteExtraLines(_contentRoot, 0);
        }

        public void UpdateRegionList(IReadOnlyList<PhotonRegionList.PhotonRegion> regions)
        {
            var regionsCount = regions.Count;
            if (_contentRoot.childCount > regionsCount)
            {
                DeleteExtraLines(_contentRoot, regionsCount);
            }
            else if (_contentRoot.childCount < regionsCount)
            {
                AddMoreTextLines(_contentRoot, regionsCount, _lineTemplate);
            }
            for (var i = 0; i < _contentRoot.childCount; ++i)
            {
                var child = _contentRoot.GetChild(i);
                UpdateTextLine(child, regions[i]);
            }
        }

        private static void UpdateTextLine(Transform parent, PhotonRegionList.PhotonRegion region)
        {
            var regionCode = parent.GetChild(0).GetComponent<TextMeshProUGUI>();
            var regionName = parent.GetChild(1).GetComponent<TextMeshProUGUI>();
            var regionPing = parent.GetChild(2).GetComponent<TextMeshProUGUI>();

            regionCode.text = region.Region;
            regionName.text = GetRegionName(region.Region);
            regionPing.text = region.Ping > 0 ? $"{region.Ping} ms" : "---   ";
        }

        private static void DeleteExtraLines(Transform parent, int minCount)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= minCount; --i)
            {
                var child = parent.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        private static void AddMoreTextLines(Transform parent, int maxCount, GameObject template)
        {
            while (parent.childCount < maxCount)
            {
                var instance = Instantiate(template, parent);
                instance.SetActive(true);
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
    }
}