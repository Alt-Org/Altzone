using System;
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

        private Action<string> _onRegionChanged;

        private readonly List<LineData> _lineDataList = new();

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
            DeleteExtraLines(0);
        }

        public void SetRegionChangedCallback(Action<string> callback)
        {
            _onRegionChanged = callback;
        }

        public void UpdateRegionList(IReadOnlyList<PhotonRegionList.PhotonRegion> regions)
        {
            var regionsCount = regions.Count;
            if (_contentRoot.childCount > regionsCount)
            {
                DeleteExtraLines(regionsCount);
            }
            else if (_contentRoot.childCount < regionsCount)
            {
                AddMoreTextLines(regionsCount);
            }
            for (var i = 0; i < _contentRoot.childCount; ++i)
            {
                _lineDataList[i].UpdateRegion(regions[i]);
            }
        }

        private void DeleteExtraLines(int minCount)
        {
            var childCount = _contentRoot.childCount;
            for (var i = childCount - 1; i >= minCount; --i)
            {
                var child = _contentRoot.GetChild(i).gameObject;
                var index = _lineDataList.FindIndex(x => x.Parent.Equals(child));
                if (index >= 0)
                {
                    _lineDataList.RemoveAt(index);
                }
                Destroy(child);
            }
        }

        private void AddMoreTextLines(int maxCount)
        {
            while (_contentRoot.childCount < maxCount)
            {
                var instance = Instantiate(_lineTemplate, _contentRoot);
                instance.SetActive(true);
                _lineDataList.Add(new LineData(instance, _onRegionChanged));
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

        private static readonly Dictionary<string, string> RegionNames = new()
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
            public readonly GameObject Parent;

            private readonly Button _button;
            private readonly TextMeshProUGUI _code;
            private readonly TextMeshProUGUI _name;
            private readonly TextMeshProUGUI _ping;

            private PhotonRegionList.PhotonRegion _region;

            public LineData(GameObject parent, Action<string> callback)
            {
                Parent = parent;
                var textComponents = parent.GetComponentsInChildren<TextMeshProUGUI>();
                // This is under button so that we can click on it.
                _name = textComponents[0];
                // These are later in hierarchy.
                _code = textComponents[1];
                _ping = textComponents[2];
                _button = parent.GetComponentsInChildren<Button>()[0];
                _button.interactable = false;
                _button.onClick.AddListener(() =>
                {
                    if (_region == null || Parent == null)
                    {
                        // No can do.
                        return;
                    }
                    callback?.Invoke(_region.Region);
                });
            }

            public void UpdateRegion(PhotonRegionList.PhotonRegion region)
            {
                _region = region;
                _code.text = region.Region;
                _name.text = GetRegionName(region.Region);
                _ping.text = region.Ping > 0 ? $"{region.Ping} ms" : "---   ";
                if (!_button.interactable)
                {
                    _button.interactable = true;
                }
            }
        }
    }
}