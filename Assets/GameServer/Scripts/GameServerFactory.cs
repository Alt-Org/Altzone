using System;

namespace GameServer.Scripts
{
    public static class GameServerFactory
    {
        public static IGameServer CreateLocal(string storageFolder)
        {
            throw new NotImplementedException();
        }
        public static IGameServer CreateRemote(string serverBaseUrl)
        {
            throw new NotImplementedException();
        }
    }
}