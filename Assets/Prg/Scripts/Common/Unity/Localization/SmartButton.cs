using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    /// <summary>
    /// Marker class for UI <c>Button</c> component localization.
    /// </summary>
    public class SmartButton : SmartText
    {
        private void Awake()
        {
            var button = GetComponentInParent<Button>();
            Assert.IsNotNull(button, "button != null");
        }
    }
}