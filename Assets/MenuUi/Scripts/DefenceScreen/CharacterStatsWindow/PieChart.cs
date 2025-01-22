using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;


namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartManager : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;

        [SerializeField] private int _sliceAmount;

        // Set the color, what kind of piece the piece should turn into according to a certain stat. These can be changed directly inside unity.
        [SerializeField] private Color impactForceColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color impactForceAltColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color healthPointsColor = Color.green;
        [SerializeField] private Color healthPointsAltColor = Color.green;
        [SerializeField] private Color defenceColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color defenceAltColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color characterSizeColor = Color.blue;
        [SerializeField] private Color characterSizeAltColor = Color.blue;
        [SerializeField] private Color speedColor = new Color(0f, 0.5f, 0f);
        [SerializeField] private Color speedAltColor = new Color(0f, 0.5f, 0f);
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color outlineColor = Color.gray;

        [SerializeField] private Sprite circleSprite;


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
            int characterSize = _controller.GetStat(StatType.Resistance);
            int speed = _controller.GetStat(StatType.Speed);

            // Get base stats
            int impactForceBase = _controller.GetBaseStat(StatType.Attack);
            int healthPointsBase = _controller.GetBaseStat(StatType.Hp);
            int defenceBase = _controller.GetBaseStat(StatType.Defence);
            int characterSizeBase = _controller.GetBaseStat(StatType.Resistance);
            int speedBase = _controller.GetBaseStat(StatType.Speed);

            speedBase = speed; // remove this once getting base speed is fixed

            // Arrange stats
            var stats = new List<(int level, Color color)>
            {
                (defenceBase, defenceColor),
                (defence - defenceBase, defenceAltColor),
                (characterSizeBase, characterSizeColor),
                (characterSize - characterSizeBase, characterSizeAltColor),
                (speedBase, speedColor),
                (speed - speedBase, speedAltColor),
                (healthPointsBase, healthPointsColor),
                (healthPoints - healthPointsBase, healthPointsAltColor),
                (impactForceBase, impactForceColor),
                (impactForce - impactForceBase, impactForceAltColor)
            };

            // Create slices
            float sliceFillAmount = 1.0f / (float)_sliceAmount;
            float currentSliceFill = 1.0f;

            int remainingSlices = _sliceAmount;

            // Colored slices
            foreach (var stat in stats)
            {
                for (int i = 0; i < stat.level; i++)
                {
                    CreateSlice(currentSliceFill, stat.color);

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
                CreateSlice(currentSliceFill, defaultColor);

                currentSliceFill -= sliceFillAmount;
            }
        }


        private void CreateSlice(float fillAmount, Color color)
        {
            // Create gameobject and add components
            GameObject slice = new GameObject();
            slice.AddComponent<RectTransform>();
            slice.AddComponent<Image>();
            slice.AddComponent<Outline>();

            // Modify image properties
            Image sliceImage = slice.GetComponent<Image>();
            sliceImage.sprite = circleSprite;
            sliceImage.color = color;
            sliceImage.type = Image.Type.Filled;
            sliceImage.fillClockwise = false;
            sliceImage.fillOrigin = (int)Image.Origin360.Top;
            sliceImage.preserveAspect = true;
            sliceImage.raycastTarget = false;
            sliceImage.fillAmount = fillAmount;

            // Outline thickness and color
            Outline sliceOutline = slice.GetComponent<Outline>();
            sliceOutline.effectDistance = new Vector2(3, -3);
            sliceOutline.effectColor = outlineColor;

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
