using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prg.Scripts.DevUtil
{
    /// <summary>
    /// Calculates FPS label once in a second in order to avoid too much garbage collection in <c>OnGUI</c>.
    /// </summary>
    public class FpsCounterLabel : MonoBehaviour
    {
        [Header("This only runs in Editor or Development Build")] public int _labelCount;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private readonly GUIStyle _guiLabelStyle = new();
        private readonly Rect _guiLabelPosition = new(5, 5, 40, 15);
        private readonly Rect _guiBoxPosition = new(3, 3, 43, 18);

        private string _fpsLabel = string.Empty;
        private readonly Dictionary<int, string> _labels = new();

        private void OnEnable()
        {
            _fpsLabel = "fps";
            StartCoroutine(TryStartFrameRateCalculator());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _fpsLabel = string.Empty;
        }

        private IEnumerator TryStartFrameRateCalculator()
        {
            if (FindObjectsOfType(GetType()).Length > 1)
            {
                enabled = false;
                yield break;
            }
            _guiLabelStyle.normal.textColor = Color.white;
            _guiLabelStyle.fontStyle = FontStyle.Bold;
            _guiLabelStyle.fontSize = AppPlatform.IsMobile || AppPlatform.IsSimulator ? 24 : 12;
            StartCoroutine(FrameRateCalculator());
        }

        private IEnumerator FrameRateCalculator()
        {
            Debug.Log("start");
            var delay = new WaitForSeconds(1.0f);
            while (enabled)
            {
                var startTime = Time.time;
                var startFrames = Time.frameCount;
                yield return delay;
                var fps = (int)((Time.frameCount - startFrames) / (Time.time - startTime) + 0.5f);
                if (!_labels.TryGetValue(fps, out var label))
                {
                    label = $"{fps} fps";
                    _labels.Add(fps, label);
                    _labelCount = _labels.Count;
                }
                _fpsLabel = label;
            }
            Debug.Log("exit");
        }

        private void OnGUI()
        {
            if (_fpsLabel == string.Empty)
            {
                return;
            }
            GUI.Box(_guiBoxPosition, string.Empty);
            GUI.Label(_guiLabelPosition, _fpsLabel, _guiLabelStyle);
        }
#endif
    }
}
