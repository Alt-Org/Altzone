using System;
using System.Collections.Generic;
using System.IO;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.LocalStorage;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class RaidGameRoomModelsTest
    {
        private const string DefaultStorageFilename = "TestRaidGameRoomModels.json";

        [OneTimeSetUp, Description("Create stable default test storage")]
        public void OneTimeSetUp()
        {
            Debug.Log($"setup {DefaultStorageFilename}");
            DeleteStorage(DefaultStorageFilename);

            var storage = new RaidGameRoomModelStorage(DefaultStorageFilename);
            Debug.Log($"storage {storage.StorageFilename}");
            var models = new List<RaidGameRoomModel>()
            {
                new(1, "test10", 7, 12),
                new(2, "test20", 7, 12),
            };
            foreach (var model in models)
            {
                SetupRoomModel(model);
                storage.Save(model);
            }

            void SetupRoomModel(RaidGameRoomModel roomModel)
            {
                // X = colum, Y = row, origo = top,left, zero based indexing
                foreach (var data in new Tuple<int,int>[]
                         {
                             new (1,2), new (5,2),
                             new (4,3),
                             new (0,4),
                             new (6,5),
                             new (4,6),
                             new (6,7),
                             new (1,9),
                             new (3,10),
                             new (4,11),
                         })
                {
                    roomModel._bombLocations.Add(new RaidGameRoomModel.BombLocation(data.Item1,data.Item2));
                }
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(0, 0, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(0, 6, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(11, 0, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(11, 6, 10));
                
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(3,0,1));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(4,0,1));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(6,0,1));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(1,6,1));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(2,7,1));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(4,8,1));
            }
        }

        [Test]
        public void DefaultStorageTest()
        {
            Debug.Log($"test {DefaultStorageFilename}");
            var storage = new RaidGameRoomModelStorage(DefaultStorageFilename);
            var models = storage.GetAll();
            Assert.IsTrue(models.Count > 1);
        }

        private static void DeleteStorage(string storageFilename)
        {
            var storagePath = Path.Combine(Application.persistentDataPath, storageFilename);
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
    }
}