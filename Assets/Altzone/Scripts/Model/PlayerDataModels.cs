using System.Collections.Generic;
using System.Threading.Tasks;
using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts.Model
{
    public interface IPlayerDataModels
    {
        Task<PlayerDataModel> GetById(int id);
        Task<List<PlayerDataModel>> GetAll();

        Task<bool> Save(PlayerDataModel item);
        Task<bool> Delete(int id);
    }

    public class PlayerDataModels : IPlayerDataModels
    {
        public Task<PlayerDataModel> GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<PlayerDataModel>> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Save(PlayerDataModel item)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}