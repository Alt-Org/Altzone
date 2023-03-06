using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GameServer.Scripts.Local
{
    internal class LocalGameServer : IGameServer
    {
        private readonly string _storageFolder;
        private readonly LocalClan _clan;
        private readonly LocalPlayer _player;

        public bool IsConnected { get; private set; }
        public string PathOrUrl => _storageFolder;
        
        public IClan Clan => _clan;
        public IPlayer Player => _player;

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
            _player = new LocalPlayer(storageFolder);
        }

        public Task<bool> Initialize()
        {
            Debug.Log($"Initialize {GetType().Name} in {_storageFolder}");
            IsConnected = true;
            return Task.FromResult(true);
        }
    }
}