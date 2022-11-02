using System.Collections;
using Altzone.Scripts.Config;
using Battle0.Scripts.Battle;
using Battle0.Scripts.Battle.Game;
using NUnit.Framework;
using Photon.Pun;
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
            // Grid based movement must be enabled.
            var runtimeGameConfig = RuntimeGameConfig.Get();
            Assert.AreEqual(true, runtimeGameConfig.Features._useBattleGridMovement);

            // We require that gameplay area is origo set on (0,0) for tests to work.
            var battlePlayArea = Context.GetBattlePlayArea;
            Assert.AreEqual(Vector2.zero, battlePlayArea.GetPlayAreaCenterPosition);

            // Use yield to skip a frame.
            var skipFrame = new WaitForEndOfFrame();
            // Wait until we are in a room because GridManager uses Photon RPC.
            while (!PhotonNetwork.InRoom)
            {
                yield return skipFrame;
            }
            Debug.Log($"Connected to {PhotonNetwork.CurrentRoom}");
            IGridManager gridManager;
            for (;;)
            {
                // Wait until grid manager is initialized.
                gridManager = Context.GetGridManager;
                if (gridManager.RowCount > 0)
                {
                    break;
                }
                yield return skipFrame;
            }
            
            var variables = runtimeGameConfig.Variables;
            var gridWidth = variables._battleUiGridWidth;
            var gridHeight = variables._battleUiGridHeight;
            
            var battleCamera = Context.GetBattleCamera;
            var world = battleCamera.Camera.ViewportToWorldPoint(Vector3.one);
            var worldWidth = 2f * world.x;
            var worldHeight = 2f * world.y;
            Debug.Log($"GRID rows {gridWidth} cols {gridHeight} WORLD width {worldWidth} height {worldHeight}");

            Assert.AreEqual(gridHeight, gridManager.RowCount);
            Assert.AreEqual(gridWidth, gridManager.ColCount);
            var rowMax = gridHeight;
            var colMax = gridWidth;
            var expectedState = true;
            var startTime = Time.time;
            foreach (var rotation in new[] { false, true })
            {
                yield return skipFrame;
                Debug.Log($"Grid rotation {rotation} expectedState {expectedState}");
                for (var row = 0; row < rowMax; ++row)
                {
                    for (var col = 0; col < colMax; ++col)
                    {
                        var gridPos = new GridPos(row, col);
                        var worldPos = gridManager.GridPositionToWorldPoint(gridPos, rotation);
                        Assert.IsFalse(Mathf.Abs(worldPos.x) > world.x);
                        Assert.IsFalse(Mathf.Abs(worldPos.y) > world.y);
                        var gridPos2 = gridManager.WorldPointToGridPosition(worldPos, rotation);
                        Assert.AreEqual(row, gridPos2.Row);
                        Assert.AreEqual(col, gridPos2.Col);
                        var isFreePosition = gridManager.GridFreeState(row, col);
                        Assert.AreEqual(expectedState, isFreePosition);
                        // Set state so that it will have opposite value on next "round".
                        if (isFreePosition)
                        {
                            gridManager.SetSpaceTaken(gridPos);
                        }
                        else
                        {
                            gridManager.SetSpaceFree(gridPos);
                        }
                    }
                }
                expectedState = !expectedState;
            }
            yield return skipFrame;
            Debug.Log($"Done in {Time.time - startTime:0.000} s");
        }
    }
}
