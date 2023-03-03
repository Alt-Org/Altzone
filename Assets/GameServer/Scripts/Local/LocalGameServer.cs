using System.Collections.Generic;

namespace GameServer.Scripts.Local
{
    internal class LocalGameServer : IGameServer
    {
        private readonly string _storageFolder;
        private readonly LocalClan _clan;

        public IClan Clan => _clan;

        public LocalGameServer(string storageFolder)
        {
            _storageFolder = storageFolder;
            _clan = new LocalClan();
        }
    }

    internal class LocalClan : IClan
    {
        public int Save()
        {
            throw new System.NotImplementedException();
        }

        public IClan Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<IClan> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}