#if USE_LOOTLOCKER
using System.Threading.Tasks;
using LootLocker.Requests;

namespace Prg.Scripts.Service.LootLocker
{
    /// <summary>
    /// Async wrapper to <c>LootLocker</c> SDK API.
    /// </summary>
    /// <remarks>
    /// We use <c>TaskCompletionSource</c> to bind caller and callee "together".
    /// </remarks>
    public static class LootLockerAsync
    {
        public static Task<LootLockerSessionResponse> StartSession(string playerIdentifier)
        {
            var taskCompletionSource = new TaskCompletionSource<LootLockerSessionResponse>();
            LootLockerSDKManager.StartSession(playerIdentifier, response => { taskCompletionSource.SetResult(response); });
            return taskCompletionSource.Task;
        }

        public static Task<LootLockerGuestSessionResponse> StartGuestSession(string playerIdentifier)
        {
            var taskCompletionSource = new TaskCompletionSource<LootLockerGuestSessionResponse>();
            LootLockerSDKManager.StartGuestSession(playerIdentifier, response => { taskCompletionSource.SetResult(response); });
            return taskCompletionSource.Task;
        }

        public static Task<string> Ping()
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            LootLockerSDKManager.Ping(r =>
            {
                var response = r.success ? $"OK date {r.date}" : r.Error;
                taskCompletionSource.SetResult(response);
            });
            return taskCompletionSource.Task;
        }
        public static Task<PlayerNameResponse> SetPlayerName(string playerName)
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.SetPlayerName(playerName, response => { taskCompletionSource.SetResult(response); });
            return taskCompletionSource.Task;
        }

        public static Task<PlayerNameResponse> GetPlayerName()
        {
            var taskCompletionSource = new TaskCompletionSource<PlayerNameResponse>();
            LootLockerSDKManager.GetPlayerName(response => { taskCompletionSource.SetResult(response); });
            return taskCompletionSource.Task;
        }
    }
}
#endif