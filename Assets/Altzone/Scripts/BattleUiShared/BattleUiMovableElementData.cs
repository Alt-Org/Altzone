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
        public int Transparency;
        public bool IsFlippedHorizontally;
        public bool IsFlippedVertically;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public OrientationType Orientation;
        public int HandleSize;

        public BattleUiMovableElementData(BattleUiElementType uiElementType, Vector2 anchorMin, Vector2 anchorMax, int transparency, OrientationType orientation = OrientationType.None, bool isFlippedHorizontally = false, bool isFlippedVertically = false, int handleSize = 0)
        {
            UiElementType = uiElementType;
            Transparency = transparency;
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Orientation = orientation;
            IsFlippedHorizontally = isFlippedHorizontally;
            IsFlippedVertically = isFlippedVertically;
            HandleSize = handleSize;
        }
    }
}
