using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;
using System.Security.Claims;


namespace MenuUi.Scripts.TopPanel
{
    public class ClanCoins : AltMonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _clanCoinsAmountText;
        private ClanData _clanData = null;

        private void OnEnable()
        {
            if (_clanData != null)
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
            PlayerData playerData = null;

            StartCoroutine(GetPlayerData(p => playerData = p));

            if (playerData != null && playerData.HasClanId)
            {
                string clanId = playerData.ClanId;
                StartCoroutine(GetClanData(p => _clanData = p, clanId));
                if (_clanData != null)
                {
                    _clanCoinsAmountText.text = _clanData.GameCoins.ToString();
                    return;
                }
            }

            _clanCoinsAmountText.text = "-";
            _clanData = null;
        }

        public void AddCoins(int addAmount)
        {
            StartCoroutine(ServerManager.Instance.AddCoinsToClan(addAmount, result =>
            {
                PlayerData playerData = null;

                StartCoroutine(GetPlayerData(p => playerData = p));
                if (playerData != null && playerData.HasClanId)
                {
                    string clanId = playerData.ClanId;
                    StartCoroutine(GetClanData(p => _clanData = p, clanId));
                    if (_clanData != null)
                    {
                        _clanData.GameCoins += addAmount;
                        _clanCoinsAmountText.text = _clanData.GameCoins.ToString();
                        Debug.Log($"Coins added: {addAmount}");
                        return;
                    }
                }
            }));
        }
    }
}
