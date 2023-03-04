using System.Collections.Generic;
using System.Threading.Tasks;
using GameServer.Scripts.Dto;

namespace GameServer.Scripts
{
    /// <summary>
    /// <c>GameServer</c> top level access point for entity services.
    /// </summary>
    public interface IGameServer
    {
        Task<bool> Initialize();
        IClan Clan { get; }
    }

    /// <summary>
    /// <c>Clan</c> entity CRUD service.
    /// </summary>
    public interface IClan
    {
        Task<bool> Save(ClanDto clan);
        Task<ClanDto> Get(int id);
        Task<List<ClanDto>> GetAll();
        Task<bool> Update(ClanDto clan);
        Task<bool> Delete(int id);
    }
}