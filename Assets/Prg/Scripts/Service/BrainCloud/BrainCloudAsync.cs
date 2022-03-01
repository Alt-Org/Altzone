#if USE_BRAINCLOUD
using System.Collections.Generic;
using System.Threading.Tasks;
using Prg.Scripts.Common.PubSub;

namespace Prg.Scripts.Service.BrainCloud
{
    /// <summary>
    /// Helper class to store some important <c>BrainCloud</c> user data for convenience.
    /// </summary>
    public class BrainCloudUser
    {
        public readonly string UserId;
        public readonly string UserName;
        public readonly string ProfileId;
        public readonly int StatusCode;

        public bool IsValid => StatusCode == 0;

        public BrainCloudUser(string userId, string userName, string profileId, int statusCode)
        {
            UserId = userId;
            UserName = userName;
            ProfileId = profileId;
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"UserName: {UserName}, ProfileId: {ProfileId}, UserId: {UserId}, StatusCode: {StatusCode}, IsValid: {IsValid}";
        }
    }

    /// <summary>
    /// Async wrapper to callback based <c>BrainCloud</c> SDK API.
    /// </summary>
    /// <remarks>
    /// We use <c>TaskCompletionSource</c> to bind caller and callee "together".
    /// </remarks>
    public static class BrainCloudAsync
    {
        private static BrainCloudWrapper _brainCloudWrapper;

        private static BrainCloudUser _brainCloudUser = new BrainCloudUser(
            string.Empty, string.Empty, string.Empty, ReasonCodes.TOKEN_DOES_NOT_MATCH_USER);

        private static readonly object Publisher = new object();

        public static BrainCloudUser BrainCloudUser => _brainCloudUser;

        public static void SetBrainCloudWrapper(BrainCloudWrapper brainCloudWrapper)
        {
            _brainCloudWrapper = brainCloudWrapper;
        }

        public static Task<int> Authenticate(string userId, string password)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            _brainCloudWrapper.AuthenticateUniversal(userId, password, true,
                (jsonData, _) =>
                {
                    Debug.Log($"Authenticate {jsonData}");
                    var data = GetJsonData(jsonData);
                    var playerName = data["playerName"].ToString();
                    var profileId = data["profileId"].ToString();
                    SetBrainCloudUser(new BrainCloudUser(userId, playerName, profileId, 0));
                    taskCompletionSource.SetResult(0);
                },
                (status, code, error, _) =>
                {
                    if (code == ReasonCodes.TOKEN_DOES_NOT_MATCH_USER)
                    {
                        Debug.Log($"Authenticate '{userId}' INCORRECT PASSWORD {status} : {code} {error}");
                    }
                    else if (code == ReasonCodes.GAME_VERSION_NOT_SUPPORTED)
                    {
                        Debug.LogWarning($"Authenticate '{userId}' GAME_VERSION_NOT_SUPPORTED {status} : {code} {error}");
                    }
                    else
                    {
                        Debug.LogWarning($"Authenticate '{userId}' FAILED {status} : {code} {error}");
                    }
                    SetBrainCloudUser(new BrainCloudUser(userId, string.Empty, string.Empty, code));
                    taskCompletionSource.SetResult(code);
                });
            return taskCompletionSource.Task;
        }

        public static Task<int> UpdateUserName(string playerNameIn)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            _brainCloudWrapper.PlayerStateService.UpdateName(playerNameIn,
                (jsonData, _) =>
                {
                    Debug.Log($"UpdateUserName {jsonData}");
                    var data = GetJsonData(jsonData);
                    var newPlayerName = data["playerName"].ToString();
                    SetBrainCloudUser(new BrainCloudUser(_brainCloudUser.UserId, newPlayerName, _brainCloudUser.ProfileId, 0));
                    taskCompletionSource.SetResult(0);
                },
                (status, code, error, _) =>
                {
                    Debug.Log($"PlayerStateService.UpdateName '{playerNameIn}' FAILED {status} : {code} {error}");
                    taskCompletionSource.SetResult(code);
                });
            return taskCompletionSource.Task;
        }

        private static void SetBrainCloudUser(BrainCloudUser user)
        {
            _brainCloudUser = user;
            Publisher.Publish(user);
        }

        private static Dictionary<string, object> GetJsonData(string jsonText)
        {
            return JsonReader.Deserialize<Dictionary<string, object>>(jsonText)["data"] as Dictionary<string, object> ??
                   new Dictionary<string, object>();
        }
    }
}
#endif