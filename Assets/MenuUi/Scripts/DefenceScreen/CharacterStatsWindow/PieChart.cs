using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using TMPro;


namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartManager : MonoBehaviour
    {
        
        [SerializeField] private PiechartReference _referenceSheet;
        [SerializeField] private TMP_Text _piechartText;

        private StatsWindowController _controller;

        private int _sliceAmount;

        private Color _impactForceColor;

        private Color _healthPointsColor;

        private Color _defenceColor;

        private Color _characterSizeColor;

        private Color _speedColor;

        private Color _defaultColor;

        private Sprite _circleSprite;
        private Sprite _circlePatternedSprite;


        private void Awake() // caching colors and circle sprite from the reference sheet to avoid unneccessary function calls
        {
            _impactForceColor = _referenceSheet.GetColor(StatType.Attack);

            _healthPointsColor = _referenceSheet.GetColor(StatType.Hp);

            _defenceColor = _referenceSheet.GetColor(StatType.Defence);

            _characterSizeColor = _referenceSheet.GetColor(StatType.CharacterSize);

            _speedColor = _referenceSheet.GetColor(StatType.Speed);

            _defaultColor = _referenceSheet.GetColor(StatType.None);

            _circleSprite = _referenceSheet.GetCircleSprite();
            _circlePatternedSprite = _referenceSheet.GetPatternedSprite();

            _sliceAmount = CustomCharacter.STATMAXCOMBINED;
        }


        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
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

            _piechartText.text = $"{impactForce+healthPoints+defence+characterSize+speed}/{_sliceAmount}";

            // Arrange stats
            var stats = new List<(int upgradesLevel, int baseLevel, Color color)>
            {
                (healthPoints - healthPointsBase, healthPointsBase, _healthPointsColor),
                (speed - speedBase, speedBase, _speedColor),
                (characterSize - characterSizeBase, characterSizeBase, _characterSizeColor),
                (impactForce - impactForceBase, impactForceBase, _impactForceColor),
                (defence - defenceBase, defenceBase, _defenceColor),
            };

            // Create slices
            float oneStatLevelFillAmount = 1.0f / (float)_sliceAmount;
            float currentSliceFill = 1.0f;

            int remainingSlices = _sliceAmount;

            // Colored slices
            foreach (var stat in stats)
            {
                // Create base slice
                CreateSlice(currentSliceFill, stat.color, true);
                currentSliceFill -= oneStatLevelFillAmount * stat.baseLevel;

                // Create upgrade slice
                CreateSlice(currentSliceFill, stat.color, false);
                currentSliceFill -= oneStatLevelFillAmount * stat.upgradesLevel;
            }

            // White slice
            CreateSlice(currentSliceFill, _defaultColor, false);
        }


        private void CreateSlice(float fillAmount, Color color, bool isBaseSlice)
        {
            // Create gameobject and add components
            GameObject slice = new GameObject();
            slice.AddComponent<RectTransform>();
            slice.AddComponent<Image>();

            // Modify image properties
            Image sliceImage = slice.GetComponent<Image>();

            if (isBaseSlice && color != _defaultColor)
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
