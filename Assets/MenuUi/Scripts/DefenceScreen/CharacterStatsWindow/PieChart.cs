using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;


namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartManager : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private PiechartReference _referenceSheet;
        [SerializeField] private int _sliceAmount;


        private Color _impactForceColor;
        private Color _impactForceAltColor;

        private Color _healthPointsColor;
        private Color _healthPointsAltColor;

        private Color _defenceColor;
        private Color _defenceAltColor;

        private Color _characterSizeColor;
        private Color _characterSizeAltColor;

        private Color _speedColor;
        private Color _speedAltColor;

        private Color _defaultColor;
        private Color _defaultAltColor;

        private Sprite _circleSprite;
        private Sprite _circlePatternedSprite;


        private void Awake() // caching colors and circle sprite from the reference sheet to avoid unneccessary function calls
        {
            _impactForceColor = _referenceSheet.GetColor(StatType.Attack);
            _impactForceAltColor = _referenceSheet.GetAlternativeColor(StatType.Attack);

            _healthPointsColor = _referenceSheet.GetColor(StatType.Hp);
            _healthPointsAltColor = _referenceSheet.GetAlternativeColor(StatType.Hp);

            _defenceColor = _referenceSheet.GetColor(StatType.Defence);
            _defenceAltColor = _referenceSheet.GetAlternativeColor(StatType.Defence);

            _characterSizeColor = _referenceSheet.GetColor(StatType.CharacterSize);
            _characterSizeAltColor = _referenceSheet.GetAlternativeColor(StatType.CharacterSize);

            _speedColor = _referenceSheet.GetColor(StatType.Speed);
            _speedAltColor = _referenceSheet.GetAlternativeColor(StatType.Speed);

            _defaultColor = _referenceSheet.GetColor(StatType.None);
            _defaultAltColor = _referenceSheet.GetAlternativeColor(StatType.None);

            _circleSprite = _referenceSheet.GetCircleSprite();
            _circlePatternedSprite = _referenceSheet.GetPatternedSprite();
        }


        private void OnEnable()
        {
            UpdateChart();
            _controller.OnStatUpdated += UpdateChart;
        }


        private void OnDisable()
        {
            _controller.OnStatUpdated -= UpdateChart;
        }


        public void UpdateChart(StatType statType = StatType.None)
        {
            // Destroy old pie slices
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // Get stats
            int impactForce = _controller.GetStat(StatType.Attack);
            int healthPoints = _controller.GetStat(StatType.Hp);
            int defence = _controller.GetStat(StatType.Defence);
            int characterSize = _controller.GetStat(StatType.CharacterSize);
            int speed = _controller.GetStat(StatType.Speed);

            // Get base stats
            int impactForceBase = _controller.GetBaseStat(StatType.Attack);
            int healthPointsBase = _controller.GetBaseStat(StatType.Hp);
            int defenceBase = _controller.GetBaseStat(StatType.Defence);
            int characterSizeBase = _controller.GetBaseStat(StatType.CharacterSize);
            int speedBase = _controller.GetBaseStat(StatType.Speed);

            // Arrange stats
            var stats = new List<(int upgradesLevel, int baseLevel, Color color, Color altColor)>
            {
                (defence - defenceBase, defenceBase, _defenceColor, _defenceAltColor),
                (characterSize - characterSizeBase, characterSizeBase, _characterSizeColor, _characterSizeAltColor),
                (speed - speedBase, speedBase, _speedColor, _speedAltColor),
                (healthPoints - healthPointsBase, healthPointsBase, _healthPointsColor, _healthPointsAltColor),
                (impactForce - impactForceBase, impactForceBase, _impactForceColor, _impactForceAltColor),
            };

            // Create slices
            float sliceFillAmount = 1.0f / (float)_sliceAmount;
            float currentSliceFill = 1.0f;

            int remainingSlices = _sliceAmount;

            // Colored slices
            foreach (var stat in stats)
            {
                // base stats
                for (int i = 0; i < stat.baseLevel; i++) 
                {
                    if (remainingSlices % 2 == 0)
                    {
                        CreateSlice(currentSliceFill, stat.color, true);
                    }
                    else
                    {
                        CreateSlice(currentSliceFill, stat.altColor, true);
                    }

                    currentSliceFill -= sliceFillAmount;

                    remainingSlices--;
                    if (remainingSlices == 0) // if runs out of slices return
                    {
                        return;
                    }
                }

                // upgraded stats
                for (int i = 0; i < stat.upgradesLevel; i++)
                {
                    if (remainingSlices % 2 == 0)
                    {
                        CreateSlice(currentSliceFill, stat.color, false);
                    }
                    else
                    {
                        CreateSlice(currentSliceFill, stat.altColor, false);
                    }

                    currentSliceFill -= sliceFillAmount;

                    remainingSlices--;
                    if (remainingSlices == 0) // if runs out of slices return
                    {
                        return;
                    }
                }
            }

            // White slices
            for (int i = remainingSlices; i > 0; i--)
            {
                if (i % 2 == 0)
                {
                    CreateSlice(currentSliceFill, _defaultAltColor, true);
                }
                else
                {
                    CreateSlice(currentSliceFill, _defaultColor, true);
                }

                currentSliceFill -= sliceFillAmount;
            }
        }


        private void CreateSlice(float fillAmount, Color color, bool isBaseSlice)
        {
            // Create gameobject and add components
            GameObject slice = new GameObject();
            slice.AddComponent<RectTransform>();
            slice.AddComponent<Image>();

            // Modify image properties
            Image sliceImage = slice.GetComponent<Image>();

            if (!isBaseSlice)
            {
                sliceImage.sprite = _circlePatternedSprite;
            }
            else
            {
                sliceImage.sprite = _circleSprite;
            }

            sliceImage.color = color;
            sliceImage.type = Image.Type.Filled;
            sliceImage.fillClockwise = false;
            sliceImage.fillOrigin = (int)Image.Origin360.Top;
            sliceImage.preserveAspect = true;
            sliceImage.raycastTarget = false;
            sliceImage.fillAmount = fillAmount;

            // Reparent to this node
            slice.transform.SetParent(transform);

            // Set scale
            slice.transform.localScale = Vector3.one;

            // Set anchors
            RectTransform sliceRect = slice.GetComponent<RectTransform>();
            sliceRect.offsetMax = Vector2.zero;
            sliceRect.offsetMin = Vector2.zero;
            sliceRect.anchorMin = Vector3.zero;
            sliceRect.anchorMax = Vector3.one;
        }
    }
}
