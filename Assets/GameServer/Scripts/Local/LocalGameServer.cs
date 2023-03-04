using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Scripts.Dto;
using UnityEngine;

namespace GameServer.Scripts.Local
{
    internal class LocalGameServer : IGameServer
    {
        private readonly string _storageFolder;
        private readonly LocalClan _clan;

        public string PathOrUrl => _storageFolder;
        public IClan Clan => _clan;

        public LocalGameServer(string storageFolder)
        {
            _storageFolder = storageFolder;
            if (!Directory.Exists(_storageFolder))
            {
                var info = Directory.CreateDirectory(_storageFolder);
                if (!info.Exists)
                {
                    throw new UnityException($"Unable to create folder {_storageFolder} for {GetType().FullName}");
                }
            }
            _clan = new LocalClan(storageFolder);
        }

        public Task<bool> Initialize()
        {
            Debug.Log($"Initialize {GetType().Name} in {_storageFolder}");
            return Task.FromResult(true);
        }
    }

    internal class LocalClan : IClan
    {
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        private readonly string _storageFilename;
        private readonly List<ClanDto> _models;

        internal LocalClan(string storageFolder)
        {
            _storageFilename = Path.Combine(storageFolder, $"{nameof(LocalClan)}.json");
            if (!File.Exists(_storageFilename))
            {
                _models = new List<ClanDto>();
                SaveStorage(_models, _storageFilename);
                return;
            }
            _models = LoadStorage(_storageFilename);
        }

        internal void Initialize()
        {
            
        }

        private static List<ClanDto> LoadStorage(string storageFilename)
        {
            var jsonData = File.ReadAllText(storageFilename, Encoding);
            var models = JsonUtility.FromJson<List<ClanDto>>(jsonData);
            return models;
        }

        private static void SaveStorage(List<ClanDto> models, string storageFilename)
        {
            var json = JsonUtility.ToJson(models);
            File.WriteAllText(storageFilename, json, Encoding);
        }

        public Task<bool> Save(ClanDto clan)
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

        public Task<ClanDto> Get(int id)
        {
            var clan = _models.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(clan);
        }

        public Task<List<ClanDto>> GetAll()
        {
            return Task.FromResult(_models);
        }

        public Task<bool> Update(ClanDto clan)
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