using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Scripts;
using GameServer.Scripts.Dto;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.GameServerTests
{
    /// <summary>
    /// Async <c>LocalGameServer</c> tests for <c>LocalClan</c> implementation.
    /// </summary>
    [TestFixture]
    public class LocalClanTest : LocalClanTestBase
    {
        [Test]
        public async Task SaveNewTest()
        {
            var clan = new ClanDto
            {
                Id = 55,
                GameCoins = 55,
                Name = "Clan-55",
                Tag = "/55"
            };
            var result = await ClanService.Save(clan);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SaveExistingTest()
        {
            var clan = new ClanDto
            {
                Id = 10,
                GameCoins = 55,
                Name = "Clan-55",
                Tag = "/55"
            };
            var result = await ClanService.Save(clan);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateNewTest()
        {
            var clan = new ClanDto
            {
                Id = 100,
                GameCoins = 100,
                Name = "Clan-100",
                Tag = "OO"
            };
            var result = await ClanService.Update(clan);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UpdateExistingTest()
        {
            var clan = new ClanDto
            {
                Id = 55,
                GameCoins = 45,
                Name = "Clan-45",
                Tag = "{45}"
            };
            var result = await ClanService.Update(clan);
            Assert.IsTrue(result);
            var updatedClan = await ClanService.Get(55);
            Assert.AreEqual(45, updatedClan.GameCoins);
            Assert.AreEqual("Clan-45", updatedClan.Name);
            Assert.AreEqual("{45}", updatedClan.Tag);
        }

        [Test]
        public async Task GetByIdOkTest()
        {
            var clan = await ClanService.Get(3);
            Assert.IsNotNull(clan);
        }

        [Test]
        public async Task GetByIdFailTest()
        {
            var clan = await ClanService.Get(33);
            Assert.IsNull(clan);
        }

        [Test]
        public async Task GetAllTest()
        {
            var clans = await GameServer.Clan.GetAll();
            Assert.IsTrue(clans.Count > 0);
        }
    }

    /// <summary>
    /// Create test data for <c>LocalClan</c>.
    /// </summary>
    [TestFixture]
    public class LocalClanTestCreate : LocalClanTestBase
    {
        [Test, Description("Initial test to create test data for later tests...")]
        public async Task CreateTestDataForTests()
        {
            Debug.Log("test");
            var clans = await ClanService.GetAll();
            // Delete all existing data by id.
            var idList = clans.Select(x => x.Id).ToList();
            foreach (var id in idList)
            {
                var result = await ClanService.Delete(id);
                Assert.IsTrue(result);
            }
            // Create known test data.
            var clanList = new List<ClanDto>
            {
                new()
                {
                    Id = 1,
                    GameCoins = 1,
                    Name = "Clan-1",
                    Tag = "[1]"
                },
                new()
                {
                    Id = 2,
                    GameCoins = 2,
                    Name = "Clan-2",
                    Tag = "[2]"
                },
                new()
                {
                    Id = 3,
                    GameCoins = 3,
                    Name = "Clan-3",
                    Tag = "[3]"
                },
                new()
                {
                    Id = 4,
                    GameCoins = 4,
                    Name = "Clan-4",
                    Tag = "[4]"
                },
                new()
                {
                    Id = 10,
                    GameCoins = 10,
                    Name = "Clan-10",
                    Tag = "[Best]"
                },
            };
            foreach (var clanDto in clanList)
            {
                Debug.Log($"save clan {clanDto.Id}");
                var result = await GameServer.Clan.Save(clanDto);
                Assert.IsTrue(result);
            }
        }
    }

    public class LocalClanTestBase
    {
        private const string GameServerTestFolder = "GameServerTest";
        protected IGameServer GameServer;
        protected IClan ClanService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
            var gameServerFolder = Path.Combine(Application.persistentDataPath, GameServerTestFolder);
            GameServer = GameServerFactory.CreateLocal(gameServerFolder);
            Assert.IsNotNull(GameServer);
            ClanService = GameServer.Clan;
            Assert.IsNotNull(ClanService);
            Initialize(GameServer);
        }

        private static async void Initialize(IGameServer gameServer)
        {
            await gameServer.Initialize();
            Debug.Log($"{gameServer.PathOrUrl}");
        }
    }
}