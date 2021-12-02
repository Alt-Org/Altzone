using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Attribute to decorate a <c>string</c> value as a <c>Tag</c> in Editor.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/WSWhitehouse/Unity-Tag-Selector
    /// </remarks>
    public class TagSelectorAttribute : PropertyAttribute
    {
        public static bool UseDefaultTagFieldDrawer = false;
    }
}