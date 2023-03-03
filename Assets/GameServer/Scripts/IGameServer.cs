using System.Collections.Generic;

namespace GameServer.Scripts
{
    /// <summary>
    /// <c>GameServer</c> top level access point for entity services.
    /// </summary>
    public interface IGameServer
    {
        IClan Clan { get; }
    }

    /// <summary>
    /// <c>Clan</c> entity CRUD service.
    /// </summary>
    public interface IClan
    {
        int Save();
        IClan Get(int id);
        List<IClan> GetAll();
        void Update();
        void Delete(int id);
    }
}