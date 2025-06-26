using System;
using UnityEngine;

using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMultiOrientationElement.OrientationType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Serializable class for holding Battle Ui Element data sunch as anchors and orientation.
    /// </summary>
    [Serializable]
    public class BattleUiMovableElementData
    {
        public BattleUiElementType UiElementType;
        public bool IsFlippedHorizontally;
        public bool IsFlippedVertically;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public OrientationType Orientation;
        public float HandleSize;

        public BattleUiMovableElementData(BattleUiElementType uiElementType, Vector2 anchorMin, Vector2 anchorMax, OrientationType orientation = OrientationType.None, bool isFlippedHorizontally = false, bool isFlippedVertically = false, float handleSize = 0)
        {
            UiElementType = uiElementType;
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Orientation = orientation;
            IsFlippedHorizontally = isFlippedHorizontally;
            IsFlippedVertically = isFlippedVertically;
            HandleSize = handleSize;
        }
    }
}
