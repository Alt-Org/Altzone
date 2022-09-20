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
            for(;;)
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
            Debug.Log("Exit");
        }
    }
}