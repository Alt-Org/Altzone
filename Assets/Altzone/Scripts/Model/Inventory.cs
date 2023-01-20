using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Inventory for <c>InventoryItem</c> objects.
    /// </summary>
    public interface IInventory
    {
        Task<IInventoryItem> GetById(int id);
        Task<IInventoryItem> GetByName(string name);
        Task<List<IInventoryItem>> GetAll();
        Task<List<IFurnitureModel>> GetAllFurnitureModelsFromInventory();

        Task<bool> Save(IInventoryItem item);
        Task<bool> Delete(int id);
    }

    public static class InventoryFactory
    {
        public static Task<IInventory> Create(string storageFilename)
        {
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

        public async Task<IInventoryItem> GetById(int id)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public async Task<IInventoryItem> GetByName(string name)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        public Task<List<IInventoryItem>> GetAll()
        {
            var taskCompletionSource = new TaskCompletionSource<List<IInventoryItem>>();
            try
            {
                var result = _itemStorage.GetAll().Cast<IInventoryItem>().ToList();
                taskCompletionSource.SetResult(result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public Task<List<IFurnitureModel>> GetAllFurnitureModelsFromInventory()
        {
            var taskCompletionSource = new TaskCompletionSource<List<IFurnitureModel>>();
            try
            {
                var items = _itemStorage.GetAll();
                var result = new List<IFurnitureModel>();
                foreach (var item in items)
                {
                    var furniture = Models.FindById<FurnitureModel>(item._furnitureId);
                    result.Add(furniture);
                }
                taskCompletionSource.SetResult(result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public Task<bool> Save(IInventoryItem item)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _itemStorage.Save(item as InventoryItem);
                taskCompletionSource.SetResult(true);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public Task<bool> Delete(int id)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _itemStorage.Delete(id);
                taskCompletionSource.SetResult(true);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }
    }
}