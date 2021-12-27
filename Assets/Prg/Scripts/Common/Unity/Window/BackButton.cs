using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Back button for <c>WindowManager</c>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                // Better have one frame delay to let other button listeners execute before actually closing current window and going back
                StartCoroutine(GoBack());
            });
        }

        private static IEnumerator GoBack()
        {
            yield return null;
            WindowManager.Get().GoBack();
        }
    }
}