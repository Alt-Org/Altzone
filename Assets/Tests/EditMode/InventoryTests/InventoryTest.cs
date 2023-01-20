using System.IO;
using System.Threading.Tasks;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Tests.EditMode.InventoryTests
{
    [TestFixture]
    public class InventoryTest
    {
        private const string InventoryItemsFilename = "TestInventoryItems.json";

        private IInventory _inventory;

        [OneTimeSetUp, Description("Create Inventory")]
        public async Task OneTimeSetUp()
        {
            Debug.Log($"setup {InventoryItemsFilename}");
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, InventoryItemsFilename);
            if (File.Exists(inventoryItemsPath))
            {
                File.Delete(inventoryItemsPath);
            }
            _inventory = await InventoryFactory.Create(inventoryItemsPath);

            // FurnitureModelLoader uses same IDs
            await _inventory.Save(new InventoryItem(10, "Test-1", 1));
            await _inventory.Save(new InventoryItem(20, "Test-2", 2));
            await _inventory.Save(new InventoryItem(30, "Test-3", 3));
            await _inventory.Save(new InventoryItem(40, "Test-4", 4));
            await _inventory.Save(new InventoryItem(50, "Test-5", 5));
            await _inventory.Save(new InventoryItem(99, "Bomb", 6));
            await _inventory.Save(new InventoryItem(70, "Test-7", 7));
            await _inventory.Save(new InventoryItem(80, "Test-8", 8));
        }

        [Test]
        public async Task CreateInventoryTest()
        {
            const string tempInventoryFilename = "TestTempInventoryItems.json";

            Debug.Log($"test {tempInventoryFilename}");
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, tempInventoryFilename);
            if (File.Exists(inventoryItemsPath))
            {
                File.Delete(inventoryItemsPath);
            }
            // Should not create empty inventory file!?
            var inventory = await InventoryFactory.Create(inventoryItemsPath);
            var fileExists = File.Exists(inventoryItemsPath);
            Assert.IsFalse(fileExists);
            
            // File should be created after Save.
            await inventory.Save(new InventoryItem(1, "Test", 1));
            fileExists = File.Exists(inventoryItemsPath);
            Assert.IsTrue(fileExists);
        }

        [Test]
        public async Task GetAllTest()
        {
            Debug.Log($"test {InventoryItemsFilename}");
            var items = await _inventory.GetAll();
            Assert.IsTrue(items.Count > 1);
            Debug.Log($"test items {items.Count}");
        }

        [Test]
        public async Task GetAllFurnitureTest()
        {
            Debug.Log($"test {InventoryItemsFilename}");
            var models = await _inventory.GetAllFurnitureModelsFromInventory();
            Assert.IsTrue(models.Count > 1);
            Debug.Log($"test models {models.Count}");
        }
    }
}