using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>IRaidGameRoomModel</c> async operations.
    /// </summary>
    public static class RaidGameRoomModels
    {
        private static RaidGameRoomModelStorage _storage;

        public static Task<bool> Connect(string storageFilename)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _storage = new RaidGameRoomModelStorage(storageFilename);
                Debug.Log($"storage {_storage.StorageFilename}");
                taskCompletionSource.SetResult(true);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"storage file {_storage.StorageFilename} error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<IRaidGameRoomModel> GetById(int id)
        {
            var taskCompletionSource = new TaskCompletionSource<IRaidGameRoomModel>();
            try
            {
                var result= _storage.GetCustomCharacterModel(id);
                taskCompletionSource.SetResult(result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<IRaidGameRoomModel> GetByName(string name)
        {
            var taskCompletionSource = new TaskCompletionSource<IRaidGameRoomModel>();
            try
            {
                var result= _storage.Find(x => x._name == name);
                if (result.Count > 1)
                {
                    throw new InvalidOperationException($"collection has {result.Count} items with '{name}'");
                }
                taskCompletionSource.SetResult(result.Count == 0 ? null : result[0]);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<List<IRaidGameRoomModel>> GetAll()
        {
            var taskCompletionSource = new TaskCompletionSource<List<IRaidGameRoomModel>>();
            try
            {
                var result= _storage.GetAll().Cast<IRaidGameRoomModel>().ToList();
                taskCompletionSource.SetResult(result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<bool> Save(RaidGameRoomModel model)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _storage.Save(model);
                taskCompletionSource.SetResult(true);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<bool> Delete(int id)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _storage.Delete(id);
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