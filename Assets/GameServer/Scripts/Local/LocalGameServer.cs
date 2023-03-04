using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Scripts.Dto;
using UnityEngine;

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

        public Task<bool> Initialize()
        {
            Debug.Log($"{nameof(LocalGameServer)} initialized");
            return Task.FromResult(true);
        }
    }

    internal class LocalClan : IClan
    {
        public Task<bool> Save(ClanDto clan)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClanDto> Get(int id)
        {
            var clan = new ClanDto()
            {
                Id = id,
                GameCoins = 0,
                Name = $"Demo{id:00}",
                Tag = "[DM]"
            };
            return Task.FromResult(clan);
        }

        public List<ClanDto> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(ClanDto clan)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}