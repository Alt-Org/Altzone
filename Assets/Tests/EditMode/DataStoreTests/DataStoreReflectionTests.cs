using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using NUnit.Framework;
using Prg.Scripts.Common.Util;
using UnityEngine.TestTools;

namespace Tests.EditMode.DataStoreTests
{
    [TestFixture]
    public class DataStoreReflectionTests
    {
        private DataStore _store;
        private LogFileWriter _logFileWriter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _logFileWriter = LogFileWriter.CreateLogFileWriter();
            Debug.SetTagsForClassName(null, null);
            Debug.Log("setup");
            _store = Storefront.Get();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            Debug.Log("exit");
            _logFileWriter.Close();
        }
        
        [UnityTest]
        public IEnumerator GetAllGameFurnitureYieldTest1()
        {
            Debug.Log("test");

            List<GameFurniture> items = null;
            var stopwatch = Stopwatch.StartNew();
            yield return _store.GetAllGameFurnitureYield(result => items = result.ToList());
            Debug.Log($"test took {stopwatch.ElapsedMilliseconds} ms");
            Assert.IsTrue(items.Count > 0);
        }

        [UnityTest]
        public IEnumerator GetAllGameFurnitureYieldTest2()
        {
            Debug.Log("test");

            List<GameFurniture> items = null;
            var stopwatch = Stopwatch.StartNew();
            yield return _store.GetAllGameFurnitureYield(result => items = result.ToList());
            Debug.Log($"test took {stopwatch.ElapsedMilliseconds} ms");
            Assert.IsTrue(items.Count > 0);
        }
    }
}