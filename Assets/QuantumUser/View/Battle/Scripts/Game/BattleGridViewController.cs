/// @file BattleGridViewController.cs
/// <summary>
/// Has a class BattleGridViewController which handles the %Battle arena grid's visuals.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle arena grid's visual functionality.

using UnityEngine;

using Battle.QSimulation.Game;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">Grid view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles %Battle arena grid's visual functionality.
    /// </summary>
    public class BattleGridViewController : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to grid's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Transform.html">Transform@u-exlink</a>.</value>
        [SerializeField] private Transform _gridParent;

        /// <value>[SerializeField] Reference to the grid cell template <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</value>
        [SerializeField] private GameObject _gridCellTemplate;

        /// <value>[SerializeField] Grid cell color A.</value>
        [SerializeField] private Color _colorA;

        /// <value>[SerializeField] Grid cell color B.</value>
        [SerializeField] private Color _colorB;

        /// @}

        /// <summary>
        /// Initializes the grid based on the static variables defined in BattleGridManager by initializing new grid cells based on #_gridCellTemplate.
        /// </summary>
        public void SetGrid()
        {
            // get scale
            float scale = (float)BattleGridManager.GridScaleFactor;

            // gridCell variables
            GameObject gridCell;
            Vector3 gridCellPosition;
            Quaternion gridCellRotation;
            Vector3 gridCellScale;
            Color gridCellColor;

            // set rotation and scale for all gridCells
            gridCellRotation = _gridCellTemplate.transform.localRotation;
            gridCellScale = new Vector3(scale, scale, scale);

            for (int row = 0; row < BattleGridManager.Rows; row++)
            {
                // skip middleArea
                if (row > BattleGridManager.TeamAlphaFieldEnd && row < BattleGridManager.TeamBetaFieldStart) row = BattleGridManager.TeamBetaFieldStart;

                for (int col = 0; col < BattleGridManager.Columns; col++)
                {
                    // set position
                    gridCellPosition = new Vector3(
                        (float)BattleGridManager.GridColToWorldXPosition(col),
                        0.0f,
                        (float)BattleGridManager.GridRowToWorldYPosition(row)
                    );

                    // set color
                    gridCellColor = (row + col) % 2 == 0 ? _colorA : _colorB;

                    // instantiate gridCell
                    gridCell = Instantiate(_gridCellTemplate, gridCellPosition, gridCellRotation, _gridParent);
                    gridCell.transform.localScale = gridCellScale;
                    gridCell.GetComponent<SpriteRenderer>().color = gridCellColor;
                    gridCell.SetActive(true);
                }
            }
        }
    }
}
