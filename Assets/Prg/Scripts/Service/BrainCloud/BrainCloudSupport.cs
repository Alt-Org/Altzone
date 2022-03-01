#if USE_BRAINCLOUD
using System;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.BrainCloud
{
    public static class BrainCloudSupport
    {
        private const string PlayerPrefsPlayerNameKey = "My.BrainCloud.PlayerName";

        public static void CreateService()
        {
            UnityExtensions.CreateGameObjectAndComponent<BrainCloudService>(nameof(BrainCloudService), true);
        }

        /// <summary>
        /// Gets BrainCLoud init params: url, secretKey, appId and version.
        /// </summary>
        /// <remarks>
        /// Both <c>secretKey</c> and <c>appId</c> are saved in StringProperty instances, see names in code.<br />
        /// See: https://portal.braincloudservers.com/login for actual values.
        /// </remarks>
        internal static string[] GetAppParams()
        {
            var secretKey = Resources.Load<StringProperty>($"{nameof(StringProperty)}.para2");
            var appId = Resources.Load<StringProperty>($"{nameof(StringProperty)}.para3");

            // AFAIK Server URL is not publicly available because they want to obscure it by themselves
            // - BrainCloudClient.s_defaultServerURL or BrainCloud.Plugin.Interface.DispatcherURL
            return new[]
            {
                "https://sharedprod.braincloudservers.com/dispatcherv2",
                secretKey.PropertyValue,
                appId.PropertyValue,
                "1.0.1"
            };
        }

        /// <summary>
        /// Gets credentials for BrainCLoud universal login: username and password.
        /// </summary>
        public static Tuple<string, string> GetCredentials()
        {
            string Reverse(string str)
            {
                var chars = str.ToCharArray();
                Array.Reverse(chars);
                return new string(chars);
            }

            var playerName = PlayerPrefs.GetString(PlayerPrefsPlayerNameKey, string.Empty);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                // Text format is: "(guid)(reversed_guid)" for new username and password.
                var guid = Guid.NewGuid().ToString();
                playerName = $"({guid})({Reverse(guid)})";
                playerName = StringSerializer.Encode(playerName);
                PlayerPrefs.SetString(PlayerPrefsPlayerNameKey, playerName);
            }
            playerName = StringSerializer.Decode(playerName);
            var tokens = playerName.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(tokens.Length == 2, "tokens.Length == 2");
            return new Tuple<string, string>(tokens[0], tokens[1]);
        }
    }
}
#endif