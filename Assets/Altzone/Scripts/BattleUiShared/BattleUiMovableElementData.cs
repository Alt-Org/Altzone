/// @file BattleUiMovableElementData.cs
/// <summary>
/// Contains @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableElementData} serializable class which holds %Battle Ui element's data to be saved.
/// </summary>
///
/// This script:<br/>
/// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Serializable.html">Serializable class@u-exlink</a> for holding Battle Ui Element data sunch as anchors and orientation to be saved.

using System;
using UnityEngine;

using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// <span class="brief-h">Movable element data <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Serializable.html">Serializable class@u-exlink</a>.</span><br/>
    /// Holds %Battle Ui Element data sunch as anchors and orientation which will be saved to <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerPrefs.html">PlayerPrefs@u-exlink</a>.
    /// </summary>
    ///
    /// Used for BattleUiMovableElement, BattleUiMultiOrientationElement and BattleUiMovableJoystickElement. Note: Not all save variables are used for each class, you can leave those as default.
    [Serializable]
    public class BattleUiMovableElementData
    {
        /// <value>%Battle Ui element's BattleUiElementType.</value>
        public BattleUiElementType UiElementType;

        /// <value>%Battle Ui element's transparency. Note: Not the same as opacity.</value>
        public int Transparency;

        /// <value>Is %Battle Ui multi orientation element flipped horizontally.</value>
        public bool IsFlippedHorizontally;

        /// <value>Is %Battle Ui multi orientation element flipped vertically.</value>
        public bool IsFlippedVertically;

        /// <value>%Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMin.html">anchorMin@u-exlink</a> for <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</value>
        public Vector2 AnchorMin;

        /// <value>%Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMax.html">anchorMax@u-exlink</a> for <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</value>
        public Vector2 AnchorMax;

        /// <value>%Battle Ui multi orientation element's OrientationType.</value>
        public OrientationType Orientation;

        /// <value>%Battle Ui joystick element's handle size.</value>
        public int HandleSize;

        /// <summary>
        /// Constructor for BattleUiMovableElementData.
        /// </summary>
        ///
        /// <param name="uiElementType">%Battle Ui element's BattleUiElementType.</param>
        /// <param name="anchorMin">%Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMin.html">anchorMin@u-exlink</a> for <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</param>
        /// <param name="anchorMax">%Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform-anchorMax.html">anchorMax@u-exlink</a> for <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</param>
        /// <param name="transparency">%Battle Ui element's transparency. Note: Not the same as opacity.</param>
        /// <param name="orientation">%Battle Ui multi orientation element's OrientationType.</param>
        /// <param name="isFlippedHorizontally">Is %Battle Ui multi orientation element flipped horizontally.</param>
        /// <param name="isFlippedVertically">Is %Battle Ui multi orientation element flipped vertically.</param>
        /// <param name="handleSize">%Battle Ui joystick element's handle size.</param>
        public BattleUiMovableElementData(BattleUiElementType uiElementType, Vector2 anchorMin, Vector2 anchorMax, int transparency, OrientationType orientation = OrientationType.None, bool isFlippedHorizontally = false, bool isFlippedVertically = false, int handleSize = 0)
        {
            UiElementType = uiElementType;
            Transparency = Math.Clamp(transparency, 0, 90);
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Orientation = orientation;
            IsFlippedHorizontally = isFlippedHorizontally;
            IsFlippedVertically = isFlippedVertically;
            HandleSize = handleSize;
        }
    }
}
