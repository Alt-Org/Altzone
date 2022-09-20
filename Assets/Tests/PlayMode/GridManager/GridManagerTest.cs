using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.PlayMode.GridManager
{
    public class GridManagerTest
    {
        [OneTimeSetUp]
        public void LoadScene()
        {
            SceneManager.LoadScene("ut-GridManagerTest");
        }

        [UnityTest]
        public IEnumerator GridManagerTestWithEnumeratorPasses()
        {
            // Use yield to skip a frame.
            IGridManager gridManager;
            for (;;)
            {
                gridManager = Context.GetGridManager;
                if (gridManager?._gridEmptySpaces != null)
                {
                    break;
                }
                yield return null;
            }
            var runtimeGameConfig = RuntimeGameConfig.Get();
            Assert.AreEqual(false, runtimeGameConfig.Features._isDisableBattleGridMovement);
            var variables = runtimeGameConfig.Variables;
            var gridWidth = variables._battleUiGridWidth;
            var gridHeight = variables._battleUiGridHeight;
            Debug.Log($"Start width {gridWidth} height {gridHeight}");

            var grid = gridManager._gridEmptySpaces;
            Assert.AreEqual(gridWidth, grid.GetLength(0));
            Assert.AreEqual(gridHeight, grid.GetLength(1));
            yield return null;
            var rowMax = gridWidth;
            var colMax = gridHeight;
            foreach (var rotation in new[] { false, true })
            {
                Debug.Log($"Grid rotation {rotation}");
                for (var row = 0; row < rowMax; ++row)
                {
                    for (var col = 0; col < colMax; ++col)
                    {
                        var worldPos = gridManager.GridPositionToWorldPoint(row, col, rotation);
                        Debug.Log($"Grid row, col {row:00},{col:00} -> x,y {worldPos.x:0.00},{worldPos.y:0.00} ({worldPos.x},{worldPos.y})");
                        var gridPos = gridManager.CalcRowAndColumn(worldPos, rotation);
                        var row2 = gridPos[0];
                        var col2 = gridPos[1];
                        Assert.AreEqual(row, row2);
                        Assert.AreEqual(col, col2);
                    }
                }
            }
            Debug.Log("Exit");
        }
    }
}