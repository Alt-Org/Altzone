using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class FurnitureSellHandler : MonoBehaviour
    {
        public StorageFurniture Furniture;

        [SerializeField] private Button _suggestSaleButton;

        private void Start()
        {
            _suggestSaleButton.onClick.AddListener(CreatePoll);
        }

        private void CreatePoll()
        {
            if (Furniture != null) PollManager.CreateFurniturePoll(FurniturePollType.Selling, Furniture);
            VotingActions.ReloadPollList?.Invoke();
        }
    }
}
