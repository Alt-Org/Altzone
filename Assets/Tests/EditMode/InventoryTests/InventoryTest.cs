using System.IO;
using System.Threading.Tasks;
using Altzone.Scripts.Model;
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
            var inventoryItemsPath = Path.Combine(Application.persistentDataPath, InventoryItemsFilename);
            if (File.Exists(inventoryItemsPath))
            {
                File.Delete(inventoryItemsPath);
            }
            _inventory = await InventoryFactory.Create(inventoryItemsPath);

            await _inventory.Save(new InventoryItem(1, "Test-1", InventoryItemType.Furniture));
            await _inventory.Save(new InventoryItem(2, "Test-2", InventoryItemType.Furniture));
            await _inventory.Save(new InventoryItem(3, "Test-3", InventoryItemType.Furniture));
            await _inventory.Save(new InventoryItem(10, "Test-10", InventoryItemType.Furniture));
            await _inventory.Save(new InventoryItem(20, "Test-20", InventoryItemType.Furniture));
            await _inventory.Save(new InventoryItem(30, "Test-30", InventoryItemType.Furniture));
        }

        [Test]
        public async Task GetAllTest()
        {
            Debug.Log($"test {InventoryItemsFilename}");
            var items = await _inventory.GetAll();
            Assert.IsTrue(items.Count > 1);
        }
    }
}