using System;
using UnityEngine;

using OrientationType = Altzone.Scripts.BattleUiShared.BattleUiMovableElement.OrientationType;

namespace Altzone.Scripts.BattleUiShared
{
    /// <summary>
    /// Serializable class for holding Battle Ui Element data sunch as anchors and orientation.
    /// </summary>
    [Serializable]
    public class BattleUiMovableElementData
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public OrientationType Orientation;
        public BattleUiMovableElementData(Vector2 anchorMin, Vector2 anchorMax, OrientationType orientation)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Orientation = orientation;
        }
    }
}

