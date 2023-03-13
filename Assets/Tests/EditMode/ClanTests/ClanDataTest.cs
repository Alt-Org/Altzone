using System.Collections;
using System.IO;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using File = UnityEngine.Windows.File;

namespace Tests.EditMode.ClanTests
{
    [TestFixture]
    public class ClanDataTest
    {
        private const string CreateClanDataTestFilename = "LocalModels_CreateClanDataTest.json";

        [UnityTest]
        public IEnumerator CreateAndUpdateClanDataTest()
        {
            Debug.Log("test");
            // We know exactly where DataStore file is created.
            var testFile = Path.Combine(Application.persistentDataPath, CreateClanDataTestFilename);
            if (File.Exists(testFile))
            {
                File.Delete(Path.Combine(Application.persistentDataPath, CreateClanDataTestFilename));
            }
            var dataStore = new DataStore(CreateClanDataTestFilename);
            ClanData clanData = null;
            var isCallbackDone = false;
            dataStore.GetClanData(1, c =>
            {
                clanData = c;
                Debug.Log($"clanData 1 {clanData}");
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);

            Assert.IsNull(clanData, "should not exist");

            clanData = new ClanData()
            {
                Id = 0,
                Name = "TestClan",
                Tag = "[=T=]",
                GameCoins = 123
            };
            isCallbackDone = false;
            ClanData newClanData = null;
            dataStore.SaveClanData(clanData, c =>
            {
                newClanData = c;
                Debug.Log($"clanData 2 {newClanData}");
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);

            Assert.IsNotNull(newClanData, "must exist");
            Assert.IsTrue(newClanData.Id != 0, "updatedClanData.Id != 0");
            // First object must have id value 1.
            Assert.AreEqual(1, newClanData.Id);

            isCallbackDone = false;
            newClanData.Name = "Updated";
            newClanData.GameCoins = 10;
            ClanData updatedClanData = null;
            dataStore.SaveClanData(newClanData, c =>
            {
                updatedClanData = c;
                Debug.Log($"clanData 3 {updatedClanData}");
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);

            Assert.IsNotNull(updatedClanData, "must exist");
            Assert.AreEqual(newClanData.Id, updatedClanData.Id);
            Assert.AreEqual("Updated", updatedClanData.Name);
            Assert.AreEqual(10, updatedClanData.GameCoins);
        }
    }
}