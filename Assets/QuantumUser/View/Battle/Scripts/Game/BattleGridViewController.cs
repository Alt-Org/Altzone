/// @file BattleGridViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Game,BattleGridViewController} class which handles the %Battle arena grid's visuals.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle arena grid's visual functionality.

// Unity usings
using UnityEngine;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">Grid view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles %Battle arena grid's visual functionality.
    /// </summary>
    public class BattleGridViewController : MonoBehaviour
    {
        /// @anchor BattleGridViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to grid's parent <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Transform.html">Transform@u-exlink</a>.</summary>
        /// @ref BattleGridViewController-SerializeFields
        [SerializeField] private Transform _gridParent;

        /// <summary>[SerializeField] Reference to the grid cell template <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        /// @ref BattleGridViewController-SerializeFields
        [SerializeField] private GameObject _gridCellTemplate;

        /// <summary>[SerializeField] Grid cell color A.</summary>
        /// @ref BattleGridViewController-SerializeFields
        [SerializeField] private Color _colorA;

        /// <summary>[SerializeField] Grid cell color B.</summary>
        /// @ref BattleGridViewController-SerializeFields
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
