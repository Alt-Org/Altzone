using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Scripts.Dto;
using UnityEngine;

namespace GameServer.Scripts.Local
{
    /// <summary>
    /// <c>LocalPlayer</c> implementation.
    /// </summary>
    public class LocalPlayer : IPlayer
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class StorageData
        {
            public List<PlayerDto> models;

            public StorageData(List<PlayerDto> models)
            {
                this.models = models;
            }
        }

        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        private readonly string _storageFilename;
        private readonly List<PlayerDto> _models;

        internal LocalPlayer(string storageFolder)
        {
            _storageFilename = Path.Combine(storageFolder, $"{nameof(LocalClan)}.json");
            if (!File.Exists(_storageFilename))
            {
                _models = new List<PlayerDto>();
                SaveStorage(_models, _storageFilename);
                return;
            }
            _models = LoadStorage(_storageFilename);
        }

        private static List<PlayerDto> LoadStorage(string storageFilename)
        {
            var jsonData = File.ReadAllText(storageFilename, Encoding);
            var data = JsonUtility.FromJson<StorageData>(jsonData);
            return data.models;
        }

        private static void SaveStorage(List<PlayerDto> models, string storageFilename)
        {
            var json = JsonUtility.ToJson(new StorageData(models));
            File.WriteAllText(storageFilename, json, Encoding);
        }

        public Task<bool> Save(PlayerDto clan)
        {
            var index = _models.FindIndex(x => x.Id == clan.Id);
            if (index >= 0)
            {
                return Task.FromResult(false);
            }
            _models.Add(clan);
            SaveStorage(_models, _storageFilename);
            return Task.FromResult(true);
        }

        public Task<PlayerDto> Get(int id)
        {
            var clan = _models.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(clan);
        }

        public Task<List<PlayerDto>> GetAll()
        {
            return Task.FromResult(_models);
        }

        public Task<bool> Update(PlayerDto clan)
        {
            var index = _models.FindIndex(x => x.Id == clan.Id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _models[index] = clan;
            SaveStorage(_models, _storageFilename);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(int id)
        {
            var index = _models.FindIndex(x => x.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }
            _models.RemoveAt(index);
            SaveStorage(_models, _storageFilename);
            return Task.FromResult(true);
        }
    }
}