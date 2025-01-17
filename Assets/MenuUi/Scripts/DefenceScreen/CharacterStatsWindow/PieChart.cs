using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartManager : MonoBehaviour
    {
        // list the slices, how many there are.
        [SerializeField] private List<Image> slices;

        [SerializeField] private StatsWindowController _controller;

        // Set the color, what kind of piece the piece should turn into according to a certain stat. These can be changed directly inside unity.
        [SerializeField] private Color impactForceColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color healthPointsColor = Color.green;
        [SerializeField] private Color defenceColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color characterSizeColor = Color.blue;
        [SerializeField] private Color speedColor = new Color(0f, 0.5f, 0f);
        [SerializeField] private Color defaultColor = Color.white;

        private void OnEnable()
        {
            // Updates PieChart when panel/page is opened.
            Debug.Log("Pie Chart Manager: Paneeli avattu, päivitetään pie chart...");
            UpdateChart();
            _controller.OnStatUpdated += UpdateChart;
        }

        private void OnDisable()
        {
            _controller.OnStatUpdated -= UpdateChart;
        }


        public void UpdateChart(StatType statType = StatType.None)
        {
            Debug.Log("Updating Pie Chart...");

            int impactForce = _controller.GetStat(StatType.Attack);
            int healthPoints = _controller.GetStat(StatType.Hp);
            int defence = _controller.GetStat(StatType.Defence);
            int characterSize = _controller.GetStat(StatType.Resistance);
            int speed = _controller.GetStat(StatType.Speed);

            // Arrange stats.
            var stats = new List<(int level, Color color)>
            {
                (speed, speedColor),
                (healthPoints, healthPointsColor),
                (impactForce, impactForceColor),
                (defence, defenceColor),
                (characterSize, characterSizeColor),
            };

            // Formats all slices.
            foreach (var slice in slices)
            {
                slice.fillAmount = 1f / slices.Count;
                slice.color = defaultColor;
            }

            // Fill up slices in order to the PieChart.
            int currentSlice = 0;

            foreach (var stat in stats)
            {
                int level = stat.level;
                Color color = stat.color;

                for (int i = 0; i < level; i++)
                {
                    if (currentSlice < slices.Count)
                    {
                        Debug.Log($"Setting Slice {currentSlice} to Color {color}");
                        slices[currentSlice].color = color;
                        currentSlice++;
                    }
                }
            }

            Debug.Log("Pie Chart updated!");
        }
    }
}
