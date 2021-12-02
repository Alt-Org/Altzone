using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Attribute to decorate an <c>int</c> value as a <c>Layer</c> in Editor.
    /// </summary>
    /// <remarks>
    /// See: https://answers.unity.com/questions/609385/type-for-layer-selection.html
    /// </remarks>
    public class LayerSelectorAttribute : PropertyAttribute
    {
    }
}