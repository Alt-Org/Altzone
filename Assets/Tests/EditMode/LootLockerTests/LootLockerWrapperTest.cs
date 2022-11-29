using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Service.LootLocker;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets.Tests.EditMode.LootLockerTests
{
    /// <summary>
    /// Test for LootLocker implementation using <c>LootLockerWrapper</c>.
    /// </summary>
    [TestFixture]
    public class LootLockerWrapperTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
            // https://console.lootlocker.com/settings/api-keys
            LootLockerWrapper.Init("1dfbd87633b925b496395555f306d754c6a6903e");
        }

        [Test]
        public void PlayerNameTest()
        {
            var playerName = LootLockerWrapper.GetPlayerName();
            Assert.IsFalse(string.IsNullOrWhiteSpace(playerName));
        }
    }
}