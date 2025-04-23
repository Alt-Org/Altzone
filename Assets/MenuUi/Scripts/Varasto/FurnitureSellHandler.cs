using System;
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
            _suggestSaleButton.onClick.AddListener(CreatePoll);
        }

        private void CreatePoll()
        {
            if (Furniture != null)
            {
                if (Furniture.ClanFurniture.InVoting) return;
                if (Furniture.ClanFurniture.VotedToSell) return;
                if (Furniture.Position != new Vector2Int(-1, -1)) return; // In soulhome

                PollManager.CreateFurniturePoll(FurniturePollType.Selling, Furniture);

                Furniture.ClanFurniture.InVoting = true;

                UpdateInfoAction?.Invoke(Furniture.ClanFurniture.InVoting);
                VotingActions.ReloadPollList?.Invoke();
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
