using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.UI
{
    /// <summary>
    /// Countdown helper script to create animated countdown text.
    /// </summary>
    /// <remarks>
    /// Countdown resolution is one second!
    /// </remarks>
    public class CountdownText : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private Gradient gradient;

        public void setCountdownValue(int value)
        {
            StopAllCoroutines();
            StartCoroutine(animateText(value));
        }

        private IEnumerator animateText(int value)
        {
            var duration = 0f;
            text.text = value.ToString("N0");
            var baseSize = text.fontSize / 3;
            float fontSize = text.fontSize;
            for (;;)
            {
                text.fontSize = baseSize + (int)(fontSize * animationCurve.Evaluate(duration));
                text.color = gradient.Evaluate(duration);
                if (duration < 1f)
                {
                    duration += Time.deltaTime;
                    yield return null;
                    continue;
                }
                yield break;
            }
        }
    }
}