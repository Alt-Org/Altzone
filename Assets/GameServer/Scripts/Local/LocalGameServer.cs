using System.IO;
using System.Threading.Tasks;
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
}