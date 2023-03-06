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
        string PathOrUrl { get; }
        bool IsConnected { get; }
        Task<bool> Initialize();
        
        IClan Clan { get; }
        IPlayer Player { get; }
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

    public interface IPlayer
    {
        Task<bool> Save(PlayerDto clan);
        Task<PlayerDto> Get(int id);
        Task<List<PlayerDto>> GetAll();
        Task<bool> Update(PlayerDto clan);
        Task<bool> Delete(int id);
    }
}