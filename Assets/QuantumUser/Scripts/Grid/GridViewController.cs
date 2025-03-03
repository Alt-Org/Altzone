using UnityEngine;

namespace Quantum
{
    public class GridViewController : MonoBehaviour
    {
        [SerializeField] private GameObject _gridCellTemplate;
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;

        public void SetGrid(int rows, int columns)
        {
            // get scale
            float scale = (float)GridManager.GridScaleFactor;

            // gridCell variables
            GameObject gridCell;
            Vector3 gridCellPosition;
            Quaternion gridCellRotation;
            Vector3 gridCellScale;
            Color gridCellColor;

            // set rotation and scale for all gridCells
            gridCellRotation = _gridCellTemplate.transform.localRotation;
            gridCellScale = new Vector3(scale, scale, scale);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // set position
                    gridCellPosition = new Vector3(
                        (float)GridManager.GridColToWorldXPosition(col),
                        0.0f,
                        (float)GridManager.GridRowToWorldYPosition(row)
                    );

                    // set color
                    gridCellColor = (row + col) % 2 == 0 ? _colorA : _colorB;

                    // instantiate gridCell
                    gridCell = Instantiate(_gridCellTemplate, gridCellPosition, gridCellRotation);
                    gridCell.transform.localScale = gridCellScale;
                    gridCell.GetComponent<SpriteRenderer>().color = gridCellColor;
                    gridCell.SetActive(true);
                }
            }
        }
    }
}
