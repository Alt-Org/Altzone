using System;
using GameServer.Scripts.Local;

namespace GameServer.Scripts
{
    public static class GameServerFactory
    {
        public static IGameServer CreateLocal(string storageFolder)
        {
            return new LocalGameServer(storageFolder);
        }

        public static IGameServer CreateRemote(string serverBaseUrl)
        {
            throw new NotImplementedException();
        }
    }
}