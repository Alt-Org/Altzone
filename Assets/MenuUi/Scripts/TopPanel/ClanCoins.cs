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
        private ClanData _clanData = null;

        private void OnEnable()
        {
            if (_clanData == null)
            {
                ClanData.OnClanInfoUpdated += SetCoinsAmountText;
            }
            
            SetCoinsAmountText();
        }

        private void OnDisable()
        {
            ClanData.OnClanInfoUpdated -= SetCoinsAmountText;
        }

        /// <summary>
        /// Sets clan coins amount text when clan info was updated.
        /// </summary>
        private void SetCoinsAmountText()
        {
            var store = Storefront.Get();
            PlayerData playerData = null;

            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);

            if (playerData != null && playerData.HasClanId)
            {
                string clanId = playerData.ClanId;
                store.GetClanData(clanId, p => _clanData = p);
                if (_clanData != null)
                {
                    _clanCoinsAmountText.text = _clanData.GameCoins.ToString();
                    return;
                }
            }

            _clanCoinsAmountText.text = "-";
            _clanData = null;
        }
    }
}