using System;
using System.Collections;
using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class FurnitureSellHandler : MonoBehaviour
    {
        public StorageFurniture Furniture;

        [SerializeField] private Button _suggestSaleButton;
        [SerializeField] private Image _suggestSaleButtonImage;
        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;

        public Action<bool> UpdateInfoAction;

        private void OnEnable()
        {
            UpdateInfoAction += UpdateButtonStatus;
            UpdateInfoAction?.Invoke(Furniture.ClanFurniture.InVoting);
        }

        private void OnDisable()
        {
            UpdateInfoAction -= UpdateButtonStatus;
        }

        private void Start()
        {
            _suggestSaleButton.onClick.AddListener(SellOrReturn);
        }

        private void SellOrReturn()
        {
            if(Furniture.ClanFurniture.VotedToSell)
                StartCoroutine(ServerManager.Instance.ReturnItemToStock(Furniture.Id, result =>
                {
                    Debug.LogWarning("Returned furniture to stock");
                    Furniture.ClanFurniture.VotedToSell = false;
                }));
            else
                StartCoroutine(CreatePoll());
        }

        private IEnumerator CreatePoll()
        {
            if (Furniture != null)
            {
                if (Furniture.ClanFurniture.InVoting) yield break;
                if (Furniture.ClanFurniture.VotedToSell) yield break;
                if (Furniture.Position != new Vector2Int(-1, -1)) yield break; // In soulhome
                bool? result = null;
                PollManager.CreateFurnitureSellPoll(FurniturePollType.Selling, Furniture, c => result = c);
                yield return new WaitUntil(() => result.HasValue);
                VotingActions.ReloadPollList?.Invoke();
                Furniture.ClanFurniture.InVoting = true;

                UpdateInfoAction?.Invoke(Furniture.ClanFurniture.InVoting);
                VotingActions.ReloadPollList?.Invoke();

                gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            }
        }

        private void UpdateButtonStatus(bool inVoting)
        {
            if (inVoting)
            {
                _suggestSaleButton.interactable = false;
                _suggestSaleButtonImage.color = _disabledColor;
            }
            else
            {
                _suggestSaleButton.interactable = true;
                _suggestSaleButtonImage.color = _enabledColor;
            }

        }
    }
}
