using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UIElements;
using System.Collections;

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


        private void Update() // updatechart has to be called every frame so that it follows swipe
        {
            UpdateChart();
        }


        public void UpdateChart()
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

            // create visuals
            _pieChart = new PieChartVisuals(transform.position, 500, 50, stats);
            GetComponent<UIDocument>().rootVisualElement.Clear();
            GetComponent<UIDocument>().rootVisualElement.Add(_pieChart);
        }
    }
}
