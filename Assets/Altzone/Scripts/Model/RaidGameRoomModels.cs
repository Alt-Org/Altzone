using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model.LocalStorage;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Helper class for external <c>RaidGameRoomModel</c> operations.
    /// </summary>
    public static class RaidGameRoomModels
    {
        private const string StorageFilename = "RaidGameRoomModels.json";

        private static RaidGameRoomModelStorage _storage;

        public static Task<bool> Connect()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                _storage = new RaidGameRoomModelStorage(StorageFilename);
                Debug.Log($"storage file {_storage.StoragePath}");
                taskCompletionSource.SetResult(true);
            }
            catch (Exception x)
            {
                Debug.LogWarning($"storage file {_storage.StoragePath} error: {x.GetType().FullName} {x.Message}");
                taskCompletionSource.SetResult(false);
            }
            return taskCompletionSource.Task;
        }

        public static RaidGameRoomModel GetById(int id)
        {
            return _storage.GetCustomCharacterModel(id);
        }

        public static RaidGameRoomModel GetByName(string name)
        {
            return _storage.GetAll().Find(x => x._name == name);
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
                taskCompletionSource.SetResult(null);
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
                taskCompletionSource.SetResult(false);
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
                taskCompletionSource.SetResult(false);
            }
            return taskCompletionSource.Task;
        }
    }
}