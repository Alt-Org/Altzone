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
                WindowManager.Get().GoBack();
            });
        }
    }
}