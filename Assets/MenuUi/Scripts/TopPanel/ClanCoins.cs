using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;


namespace MenuUi.Scripts.TopPanel
{
    public class ClanCoins : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _clanCoinsAmountText;
        private ServerPlayer _player;

        private void OnEnable()
        {
            ServerManager.OnLogInStatusChanged += SetCoinsAmountText;
            _player = ServerManager.Instance.Player;

            if (_player == null)
                SetCoinsAmountText(false);
            else
                SetCoinsAmountText(true);
        }

        private void OnDisable()
        {
            ServerManager.OnLogInStatusChanged -= SetCoinsAmountText;
        }

        /// <summary>
        /// Sets clan coins when login status changes. (Will be changed later on to a different event when clan coin systems get developed.)
        /// </summary>
        /// <param name="isLoggedIn">Logged in status</param>
        private void SetCoinsAmountText(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                var store = Storefront.Get();
                PlayerData playerData = null;

                store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);

                ClanData clanData = null;
                string clanId = "";

                if (playerData.HasClanId)
                {
                    clanId = playerData.ClanId;
                    store.GetClanData(clanId, p => clanData = p);
                    if (clanData != null)
                    {
                        _clanCoinsAmountText.text = clanData.GameCoins.ToString();
                        return;
                    }
                }
            }

            _clanCoinsAmountText.text = "-";

        }
    }
}
