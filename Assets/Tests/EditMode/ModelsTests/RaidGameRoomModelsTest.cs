using System;
using System.Collections.Generic;
using System.IO;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.LocalStorage;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.ModelsTests
{
    [TestFixture]
    public class RaidGameRoomModelsTest
    {
        private const string DefaultStorageFilename = "TestClanGameRoomModels.json";

        [OneTimeSetUp, Description("Create stable default test storage")]
        public void OneTimeSetUp()
        {
            Debug.Log($"setup {DefaultStorageFilename}");
            DeleteStorage(DefaultStorageFilename);

            var storage = new RaidGameRoomModelStorage(GetStoragePath(DefaultStorageFilename));
            Debug.Log($"storage {storage.StorageFilename}");
            var models = new List<RaidGameRoomModel>()
            {
                CreateRoomModelTest(new RaidGameRoomModel(1, "test10", 7, 12, 10)),
                CreateRoomModelAbba(new(2, "Abba", 7, 12, 20)),
            };
            foreach (var model in models)
            {
                storage.Save(model);
            }

            RaidGameRoomModel CreateRoomModelTest(RaidGameRoomModel roomModel)
            {
                // X = colum, Y = row, origo = top,left, zero based indexing
                foreach (var data in new Tuple<int, int>[]
                         {
                             new(1, 2), new(5, 2),
                             new(4, 3),
                             new(0, 4),
                             new(6, 5),
                             new(4, 6),
                             new(6, 7),
                             new(1, 9),
                             new(3, 10),
                             new(4, 11),
                         })
                {
                    const int bombId = 6;
                    roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(data.Item1, data.Item2, 0, 0, bombId));
                }
                roomModel._freeSpaceLocations.Add(new RaidGameRoomModel.FreeSpaceLocation(1, 0));
                roomModel._freeSpaceLocations.Add(new RaidGameRoomModel.FreeSpaceLocation(6, 9));
                roomModel._freeSpaceLocations.Add(new RaidGameRoomModel.FreeSpaceLocation(1, 11));

                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(0, 0, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(6, 0, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(0, 11, 10));
                roomModel._coinLocations.Add(new RaidGameRoomModel.CoinLocation(6, 11, 10));

                const int squareId = 1;
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(3, 0, 0, 0, squareId));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(4, 0, 0, 0, squareId));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(6, 0, 0, 0, squareId));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(1, 6, 0, 0, squareId));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(2, 7, 0, 0, squareId));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(4, 8, 0, 0, squareId));
                return roomModel;
            }

            RaidGameRoomModel CreateRoomModelAbba(RaidGameRoomModel roomModel)
            {
                const int twoH = 2;
                const int twoV = 3;
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(0, 3, 0, 0, twoH));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(0, 3, 90, 0, twoV));

                const int letterA = 9;
                const int letterB = 10;
                const int bombId = 6;
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(-2, 1, 0, 0, letterA));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(-1, 1, 0, 0, letterB));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(1, 1, 0, 0, letterB));
                roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(2, 1, 0, 0, letterA));

                foreach (var x in new int[] { -4, -2, 0, 2, 4 })
                {
                    roomModel._furnitureLocations.Add(new RaidGameRoomModel.FurnitureLocation(x, -1, 0, 0, bombId));
                }
                return roomModel;
            }
        }

        [Test]
        public void DefaultStorageTest()
        {
            Debug.Log($"test {DefaultStorageFilename}");
            var storage = new RaidGameRoomModelStorage(GetStoragePath(DefaultStorageFilename));
            var models = storage.GetAll();
            Assert.IsTrue(models.Count > 1);
        }

        private static string GetStoragePath(string storageFilename)
        {
            return Path.Combine(Application.persistentDataPath, storageFilename);
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