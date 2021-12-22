using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    /// <summary>
    /// Marker class for UI <c>Toggle</c> component localization.
    /// </summary>
    public class SmartToggle : SmartText
    {
        private void Awake()
        {
            var toggle = GetComponentInParent<Toggle>();
            Assert.IsNotNull(toggle, "toggle != null");
        }
    }
}