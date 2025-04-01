using System;
using UnityEngine;

namespace Altzone.Scripts.BattleUi
{
    /// <summary>
    /// Serializable class for holding Battle Ui Element data sunch as anchors and orientation.
    /// </summary>
    [Serializable]
    public class BattleUiElementData
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public BattleUiElementOrientation Orientation;
        public BattleUiElementData(Vector2 anchorMin, Vector2 anchorMax, BattleUiElementOrientation orientation)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Orientation = orientation;
        }
    }
}

