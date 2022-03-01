#if USE_BRAINCLOUD
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.BrainCloud
{
    public class BrainCloudService : MonoBehaviour
    {
        private static BrainCloudService _instance;
        [SerializeField] private BrainCloudWrapper _brainCloudWrapper;

        public static BrainCloudService Get() => _instance;

        public bool IsReady => BrainCloudAsync.BrainCloudUser != null;

        public BrainCloudUser BrainCloudUser => BrainCloudAsync.BrainCloudUser;

        private async void Awake()
        {
            Debug.Log("Awake");
            Assert.IsTrue(_instance == null, "_instance == null");
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _brainCloudWrapper = gameObject.AddComponent<BrainCloudWrapper>();
            BrainCloudAsync.SetBrainCloudWrapper(_brainCloudWrapper);
            Init(BrainCloudSupport.GetAppParams());
        }

        /// <summary>
        /// Initializes BrainCLoud.<br />
        /// See: https://getbraincloud.com/apidocs/tutorials/c-sharp-tutorials/getting-started-with-c-sharp/
        /// </summary>
        private void Init(string[] @params)
        {
            Debug.Log("Init");
            var i = -1;
            var url = @params[++i];
            var secretKey = @params[++i];
            var appId = @params[++i];
            var version = @params[++i];
            Assert.IsTrue(!string.IsNullOrWhiteSpace(secretKey), "!string.IsNullOrWhiteSpace(secretKey)");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(appId), "!string.IsNullOrWhiteSpace(appId)");
            _brainCloudWrapper.Init(url, secretKey, appId, version);
            // Compress messages larger than 50Kb (default value).
            var client = _brainCloudWrapper.Client;
            client.EnableCompressedRequests(true);
            client.EnableCompressedResponses(true);
        }

        #region Wrapper methods for BrainCloudAsync

        /// <summary>
        /// Authenticates a user using universal authentication.
        /// </summary>
        /// <remarks>
        /// Will create a new user if none exists!
        /// </remarks>
        public async Task<bool> Authenticate(string userId, string password)
        {
            Debug.Log($"Authenticate '{userId}'");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(userId), "!string.IsNullOrWhiteSpace(userId)");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(password), "!string.IsNullOrWhiteSpace(password)");
            var result = await BrainCloudAsync.Authenticate(userId, password);
            Debug.Log($"Authenticate {result}");
            return result == 0;
        }

        /// <summary>
        /// Sets current BrainCloud user's profile's username.
        /// </summary>
        public async Task<bool> UpdateUserName(string playerName)
        {
            Debug.Log($"UpdateUserName '{BrainCloudAsync.BrainCloudUser.UserName}' <- '{playerName}'");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(playerName), "!string.IsNullOrWhiteSpace(playerName)");
            var result = await BrainCloudAsync.UpdateUserName(playerName);
            Debug.Log($"UpdateUserName {result}");
            return result == 0;
        }

        #endregion
    }
}
#endif