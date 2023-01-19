using System;
using Altzone.Scripts.Config;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.EditMode.ConfigTests
{
    /// <summary>
    /// Note that these tests should not change, add or delete existing data which makes these a bit poor tests.
    /// </summary>
    /// <remarks>
    /// If <c>IsFirstTimePlaying</c> is true and <c>IsAccountVerified</c> is false we can do destructive testing!<br />
    /// Also note that <c>PlayerDataCache</c> is cached and will not reset 'easily'.
    /// </remarks>
    [TestFixture]
    public class PlayerDataCacheTest
    {
        [Test]
        public void NonDestructiveTest1()
        {
            Debug.Log($"test");
            var playerDataCache = GameConfig.Get().PlayerSettings;

            Assert.IsTrue(playerDataCache.HasPlayerName);

            // Execute every getter.
            var playerName = playerDataCache.PlayerName;
            var playerGuid = playerDataCache.PlayerGuid;
            var clanId = playerDataCache.ClanId;
            var customCharacterModelId = playerDataCache.CustomCharacterModelId;
            var language = playerDataCache.Language;
            var isDebugFlag = playerDataCache.IsDebugFlag;
            var isTosAccepted = playerDataCache.IsTosAccepted;
            var isFirstTimePlaying = playerDataCache.IsFirstTimePlaying;
            var isAccountVerified = playerDataCache.IsAccountVerified;
        }

        [Test]
        public void DestructiveTest()
        {
            const int dummyModelId = -1;
            
            var playerDataCache = GameConfig.Get().PlayerSettings;
            if (!playerDataCache.IsFirstTimePlaying && playerDataCache.IsAccountVerified)
            {
                Debug.Log($"test SKIPPED");
                return;
            }
            Debug.Log($"test");
            var customCharacterModelId = playerDataCache.CustomCharacterModelId;
            if (customCharacterModelId <= 0)
            {
                playerDataCache.SetCustomCharacterModelId(1);
                Assert.AreEqual(1, playerDataCache.CustomCharacterModelId);
            }
            else
            {
                playerDataCache.SetCustomCharacterModelId(-123);
                Assert.AreEqual(dummyModelId, playerDataCache.CustomCharacterModelId);
            }
            var language = playerDataCache.Language;
            if (language == SystemLanguage.English)
            {
                playerDataCache.Language = SystemLanguage.Finnish;
                Assert.AreEqual(SystemLanguage.Finnish, playerDataCache.Language);
            }
            else
            {
                playerDataCache.Language = SystemLanguage.English;
                Assert.AreEqual(SystemLanguage.English, playerDataCache.Language);
            }
            var isDebugFlag = !playerDataCache.IsDebugFlag;
            playerDataCache.IsDebugFlag = isDebugFlag;
            Assert.AreEqual(isDebugFlag, playerDataCache.IsDebugFlag);

            var isTosAccepted = !playerDataCache.IsTosAccepted;
            playerDataCache.IsTosAccepted = isTosAccepted;
            Assert.AreEqual(isTosAccepted, playerDataCache.IsTosAccepted);

            var isFirstTimePlaying = !playerDataCache.IsFirstTimePlaying;
            playerDataCache.IsFirstTimePlaying = isFirstTimePlaying;
            Assert.AreEqual(isFirstTimePlaying, playerDataCache.IsFirstTimePlaying);

            var isAccountVerified = !playerDataCache.IsAccountVerified;
            playerDataCache.IsAccountVerified = isAccountVerified;
            Assert.AreEqual(isAccountVerified, playerDataCache.IsAccountVerified);

            // Keep PlayerName.
            var playerName = playerDataCache.PlayerName;
            const string name = nameof(PlayerDataCacheTest);
            playerDataCache.SetPlayerName(name);
            Assert.AreEqual(name, playerDataCache.PlayerName);
            playerDataCache.SetPlayerName(playerName);

            playerDataCache.SetClanId(1);
            Assert.AreEqual(1, playerDataCache.ClanId);

            playerDataCache.SetCustomCharacterModelId(1);
            Assert.AreEqual(1, playerDataCache.CustomCharacterModelId);

            // Keep GUID.
            var playerGuid = playerDataCache.PlayerGuid;
            Assert.IsFalse(string.IsNullOrWhiteSpace(playerGuid));
            var tempGuid = Guid.NewGuid().ToString();
            playerDataCache.SetPlayerGuid(tempGuid);
            Assert.AreEqual(tempGuid, playerDataCache.PlayerGuid);
            playerDataCache.SetPlayerGuid(playerGuid);

            Debug.Log($"done {playerDataCache}");
        }
    }
}