using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.CanvasUtil
{
    /// <summary>
    /// Sets <c>CanvasScaler</c> matchWidthOrHeight value based on screen width and height in order to force portrait or landscape match value. 
    /// </summary>
    /// <remarks>
    /// Requires <c>ScaleMode.ScaleWithScreenSize</c> to work!<br />
    /// This is stupid way to poll for changes in game window width or height using screen resolution as measurement stick.
    /// </remarks>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerAutoMatch : MonoBehaviour
    {
        private const float DefaultLandscapeMatch = 0f;
        private const float DefaultPortraitMatch = 1f;

        [Header("Settings"), SerializeField] private float _landscapeMatch = DefaultLandscapeMatch;
        [SerializeField] private float _portraitMatch = DefaultPortraitMatch;
        [SerializeField] private float _pollingInterval = 1.0f;

        private void OnEnable()
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                enabled = false;
                return;
            }
            if (AppPlatform.IsEditor)
            {
                StartCoroutine(ScreenResolutionPoller(canvasScaler));
                return;
            }
            FixCanvasScaler(canvasScaler, _landscapeMatch, _portraitMatch);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private static void FixCanvasScaler(CanvasScaler canvasScaler, float landscapeMatch, float portraitMatch)
        {
            var match = Screen.width > Screen.height ? landscapeMatch : portraitMatch;
            if (Mathf.Approximately(canvasScaler.matchWidthOrHeight, match))
            {
                return;
            }
            Debug.Log($"screen w={Screen.width} h={Screen.height} matchWidthOrHeight {canvasScaler.matchWidthOrHeight:0.0} <- {match:0.0}",
                canvasScaler.gameObject);
            canvasScaler.matchWidthOrHeight = match;
        }

        private IEnumerator ScreenResolutionPoller(CanvasScaler canvasScaler)
        {
            var width = 0;
            var height = 0;
            YieldInstruction delay = _pollingInterval > 0 ? new WaitForSeconds(_pollingInterval) : null;
            for (; enabled;)
            {
                yield return delay;
                if (height == Screen.height && width == Screen.width)
                {
                    continue;
                }
                width = Screen.width;
                height = Screen.height;
                FixCanvasScaler(canvasScaler, _landscapeMatch, _portraitMatch);
            }
        }
    }
}
