using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class PieChartVisuals : VisualElement
    {
        private Vector2 _center;
        private float _radius;
        private int _totalSegmentAmount;
        private List<(int amount, Color color)> _coloredSegments;

        /// <summary>
        /// Create pie chart visuals.
        /// </summary>
        /// <param name="center">The center coordinates for the piechart</param>
        /// <param name="radius">Piechart radius</param>
        /// <param name="totalSegmentAmount">Amount of segments for the piechart</param>
        /// <param name="coloredSegments">List of colored segments in piechart. Each element has amount for how many piechart segments to color and the color which the segments will be.</param>
        public PieChartVisuals(Vector2 center, float radius, int totalSegmentAmount, List<(int amount, Color color)> coloredSegments = null)
        {
            _center = center;
            _radius = radius;
            _totalSegmentAmount = totalSegmentAmount;
            _coloredSegments = coloredSegments;

            generateVisualContent += DrawPieChart;
        }

        void DrawPieChart(MeshGenerationContext ctx)
        {
            // creating gradient for the stroke so the black dot in the center won't be too large
            var gradient = new Gradient();

            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(Color.black, 1.0f);
            colors[1] = new GradientColorKey(Color.black, 0.0f);

            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
            alphas[1] = new GradientAlphaKey(0.0f, 0.0f);

            gradient.SetKeys(colors, alphas);

            // creating painter and defining parameters
            var painter2D = ctx.painter2D;
            painter2D.lineWidth = 0.5f;
            painter2D.strokeGradient = gradient;

            // getting the angle for one segment
            float segmentAngle = 360.0f / (float)_totalSegmentAmount;

            // initializing startAngle and endAngle variables which are passed to painter2D's Arc method. -90.0f is to start filling segments from the top instead of right middle.
            Angle startAngle = -90.0f;
            Angle endAngle = -90.0f + segmentAngle;

            // drawing coloredSegments first
            int remainingTotalSegments = _totalSegmentAmount;

            foreach ( var segment in _coloredSegments )
            {
                painter2D.fillColor = segment.color;

                for (int i = 0; i < segment.amount; i++)
                {
                    painter2D.BeginPath();

                    // Move to the arc center
                    painter2D.MoveTo(_center);

                    // Draw the arc, and close the path
                    painter2D.Arc(_center, _radius, startAngle, endAngle);
                    painter2D.ClosePath();

                    // Fill and stroke the path
                    painter2D.Fill(FillRule.OddEven);
                    painter2D.Stroke();

                    // Increasing angles for drawing next segment
                    startAngle.value += segmentAngle;
                    endAngle.value += segmentAngle;

                    remainingTotalSegments--;
                    if ( remainingTotalSegments == 0 ) // if runs out of segments return
                    {
                        return;
                    }
                }
            }

            // drawing white segments
            painter2D.fillColor = Color.white;

            for (int i = 0; i < remainingTotalSegments; i++)
            {
                painter2D.BeginPath();

                // Move to the arc center
                painter2D.MoveTo(_center);

                // Draw the arc, and close the path
                painter2D.Arc(_center, _radius, startAngle, endAngle);
                painter2D.ClosePath();

                // Fill and stroke the path
                painter2D.Fill(FillRule.OddEven);
                painter2D.Stroke();

                // Increasing angles for drawing next segment
                startAngle.value += segmentAngle;
                endAngle.value += segmentAngle;
            }
        }
    }
}

