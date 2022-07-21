using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Tests
{
    [RequireComponent(typeof(Button))]
    public class FullScreenButtonToggle : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                StartCoroutine(UpdateCaption(button));
            });
            StartCoroutine(UpdateCaption(button));
        }

        private static IEnumerator UpdateCaption(Button button)
        {
            yield return null;
            var text = Screen.fullScreen ? "FULLSCREEN" : "WINDOW";
            button.SetCaption(text);
        }
    }
}