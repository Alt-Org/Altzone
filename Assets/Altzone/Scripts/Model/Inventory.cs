using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Inventory for <c>InventoryItem</c> objects.
    /// </summary>
    public interface IInventory
    {
        Task<InventoryItem> GetById(int id);
        Task<InventoryItem> GetByName(string name);
        Task<List<InventoryItem>> GetByType(InventoryItemType type);
        Task<List<InventoryItem>> GetAll();

        Task<bool> Save(InventoryItem item);
        Task<bool> Delete(int id);
    }

    public static class InventoryFactory
    {
        public static Task<IInventory> Create(string storageFilename)
        {
            Debug.Log("here");
            var taskCompletionSource = new TaskCompletionSource<IInventory>();
            try
            {
                var inventory = new Inventory(storageFilename);
                Debug.Log($"inventory {inventory.StorageFilename}");
                taskCompletionSource.SetResult(inventory);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }
    }
    
    internal class Inventory : IInventory
    {
        private readonly InventoryItemStorage<InventoryItem> _itemStorage;

        public string StorageFilename => _itemStorage.StorageFilename;

        public Inventory(string storageFilename)
        {
            _itemStorage = new InventoryItemStorage<InventoryItem>(storageFilename);
        }

        public async Task<InventoryItem> GetById(int id)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<InventoryItem> GetByName(string name)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<List<InventoryItem>> GetByType(InventoryItemType type)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<List<InventoryItem>> GetAll()
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<bool> Save(InventoryItem item)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<bool> Delete(int id)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}