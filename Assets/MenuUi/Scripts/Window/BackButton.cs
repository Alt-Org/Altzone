using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    /// <summary>
    /// Back button for <c>WindowManager</c>.
    /// </summary>
    /// <remarks>
    /// Click handler has one frame delay to let other button listeners execute
    /// before actually closing the current window and going back.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() => StartCoroutine(OnClick()));
        }

        private static IEnumerator OnClick()
        {
            yield return null;
            WindowManager.Get().GoBack();
        }
    }
}
