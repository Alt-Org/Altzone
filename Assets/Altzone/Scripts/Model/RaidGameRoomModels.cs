using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>RaidGameRoomModel</c> async operations.
    /// </summary>
    public static class RaidGameRoomModels
    {
        private static RaidGameRoomModelStorage _storage;

        public static Task<bool> Connect(string storageFilename)
        {
            Debug.Log("here");
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

        public static Task<RaidGameRoomModel> GetById(int id)
        {
            var taskCompletionSource = new TaskCompletionSource<RaidGameRoomModel>();
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

        public static Task<RaidGameRoomModel> GetByName(string name)
        {
            var taskCompletionSource = new TaskCompletionSource<RaidGameRoomModel>();
            try
            {
                var result= _storage.GetAll().Find(x => x._name == name);
                taskCompletionSource.SetResult(result);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetException(x);
            }
            return taskCompletionSource.Task;
        }

        public static Task<List<RaidGameRoomModel>> GetAll()
        {
            var taskCompletionSource = new TaskCompletionSource<List<RaidGameRoomModel>>();
            try
            {
                var result= _storage.GetAll();
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