using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UIElements;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartManager : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;

        // Set the color, what kind of piece the piece should turn into according to a certain stat. These can be changed directly inside unity.
        [SerializeField] private Color impactForceColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color healthPointsColor = Color.green;
        [SerializeField] private Color defenceColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color characterSizeColor = Color.blue;
        [SerializeField] private Color speedColor = new Color(0f, 0.5f, 0f);
        [SerializeField] private Color defaultColor = Color.white;

        PieChartVisuals _pieChart;

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
            int impactForce = _controller.GetStat(StatType.Attack);
            int healthPoints = _controller.GetStat(StatType.Hp);
            int defence = _controller.GetStat(StatType.Defence);
            int characterSize = _controller.GetStat(StatType.Resistance);
            int speed = _controller.GetStat(StatType.Speed);

            // Arrange stats.
            var stats = new List<(int level, Color color)>
            {
                (defence, defenceColor),
                (characterSize, characterSizeColor),
                (speed, speedColor),
                (healthPoints, healthPointsColor),
                (impactForce, impactForceColor),
            };
            RectTransform rect = GetComponent<RectTransform>();
            _pieChart = new PieChartVisuals(new Vector2(107, 275), 40, 50, stats);
            GetComponent<UIDocument>().rootVisualElement.Clear();
            GetComponent<UIDocument>().rootVisualElement.Add(_pieChart);
        }
    }
}
