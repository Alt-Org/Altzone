using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.LocalStorage;
using Altzone.Scripts.Model.ModelStorage;
using Altzone.Scripts.Model.Store;

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
                var inventory = new InventoryAsync(storageFilename);
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
}