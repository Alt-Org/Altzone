using UnityEngine;

using Battle.QSimulation.Game;

namespace Battle.View.Game
{
    public class BattleGridViewController : MonoBehaviour
    {
        [SerializeField] private Transform _gridParent;
        [SerializeField] private GameObject _gridCellTemplate;
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;

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
