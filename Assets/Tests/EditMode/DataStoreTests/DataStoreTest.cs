using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using NUnit.Framework;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.DataStoreTests
{
    [TestFixture]
    public class DataStoreTest
    {
        private DataStore _store;
        private LogFileWriter _logFileWriter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _logFileWriter = LogFileWriter.CreateLogFileWriter();
            Debug.Log("setup");
            _store = Storefront.Get();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            Debug.Log("exit");
            _logFileWriter.Close();
        }

        [UnityTest, Description("Gets Player Data and optionally Clan Data")]
        public IEnumerator GetPlayerAndClanTest()
        {
            Debug.Log("test");
            yield return null;
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;

            PlayerData playerData = null;
            var isCallbackDone = false;
            _store.GetPlayerData(playerGuid, result =>
            {
                playerData = result;
                Debug.Log($"{result?.GetType().Name} {result}");
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsNotNull(playerData);

            if (!playerData.HasClanId)
            {
                yield break;
            }
            ClanData clanData = null;
            isCallbackDone = false;
            _store.GetClanData(playerData.ClanId, result =>
            {
                clanData = result;
                Debug.Log($"{result?.GetType().Name} {result}");
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsNotNull(clanData);
        }

        [UnityTest, Description("Gets all BattleCharacters")]
        public IEnumerator GetAllBattleCharactersTest()
        {
            Debug.Log("test of BattleCharacter");
            List<BattleCharacter> battleCharacters = null;
            var isCallbackDone = false;
            _store.GetAllBattleCharactersTest(result =>
            {
                battleCharacters = result;
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsTrue(battleCharacters.Count > 0);
            foreach (var item in battleCharacters)
            {
                Debug.Log(item.ToString());
            }
        }

        [UnityTest, Description("Gets all CharacterClasses")]
        public IEnumerator GetAllCharacterClassesTest()
        {
            Debug.Log("test of CharacterClass");
            List<CharacterClass> characterClasses = null;
            var isCallbackDone = false;
            _store.GetAllCharacterClasses(result =>
            {
                characterClasses = result.ToList();
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsTrue(characterClasses.Count > 0);
            foreach (var item in characterClasses)
            {
                Debug.Log(item.ToString());
            }
        }

        [UnityTest, Description("Gets all CustomCharacters")]
        public IEnumerator GetAllCustomCharactersTest()
        {
            Debug.Log("test of CustomCharacter");
            List<CustomCharacter> customCharacters = null;
            var isCallbackDone = false;
            _store.GetAllCustomCharactersTest(result =>
            {
                customCharacters = result;
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsTrue(customCharacters.Count > 0);
            foreach (var item in customCharacters)
            {
                Debug.Log(item.ToString());
            }
        }

        [UnityTest, Description("Gets all GameFurniture")]
        public IEnumerator GetAllGameFurnitureTest()
        {
            Debug.Log("test of GameFurniture");
            List<GameFurniture> gameFurniture = null;
            var isCallbackDone = false;
            _store.GetAllGameFurniture(result =>
            {
                gameFurniture = result.ToList();
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);
            Assert.IsTrue(gameFurniture.Count > 0);
            foreach (var item in gameFurniture)
            {
                Debug.Log(item.ToString());
            }
        }
    }
}