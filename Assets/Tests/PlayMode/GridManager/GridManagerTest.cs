using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using NUnit.Framework;
using UnityEngine;
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
            
            var battleCamera = Context.GetBattleCamera;
            var world = battleCamera.Camera.ViewportToWorldPoint(Vector3.one);
            var worldWidth = 2f * world.x;
            var worldHeight = 2f * world.y;
            Debug.Log($"GRID rows {gridWidth} cols {gridHeight} WORLD width {worldWidth} height {worldHeight}");

            // We require that play are is set on origo (0,0) - to check that world pos is inside our "grid" world
            var battlePlayArea = Context.GetBattlePlayArea;
            Assert.AreEqual(Vector2.zero, battlePlayArea.GetPlayAreaCenterPosition);

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
                        GridPos gridPos = new GridPos(row, col);
                        var worldPos = gridManager.GridPositionToWorldPoint(gridPos, rotation);
                        Debug.Log($"Grid row, col {row:00},{col:00} -> x,y {worldPos.x:0.00},{worldPos.y:0.00} ({worldPos.x},{worldPos.y})");
                        Assert.IsFalse(Mathf.Abs(worldPos.x) > world.x);
                        Assert.IsFalse(Mathf.Abs(worldPos.y) > world.y);
                        var gridPos2 = gridManager.WorldPointToGridPosition(worldPos, rotation);
                        var row2 = gridPos2.Row;
                        var col2 = gridPos2.Col;
                        Assert.AreEqual(row, row2);
                        Assert.AreEqual(col, col2);
                    }
                }
            }
            Debug.Log("Done");
        }
    }
}
