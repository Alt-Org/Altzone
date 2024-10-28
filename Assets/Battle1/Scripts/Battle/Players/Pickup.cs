using Altzone.Scripts.Model.Poco.Game;
using Battle1.Scripts.Battle.Game;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players
{
    internal class Pickup : MonoBehaviour
    {

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
        }

        private IReadOnlyBattlePlayer _battlePlayer;
        private DiamondController _diamondController;

        private void Start()
        {
            _diamondController = FindObjectOfType<DiamondController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject obj = collision.gameObject;
            if (obj.CompareTag("BattleDiamond"))
            {
                DiamondType diamondType = obj.GetComponent<Diamond>().GetDiamondType();
                _diamondController.OnDiamondPickup(diamondType, _battlePlayer.BattleTeam.TeamNumber);
                collision.gameObject.SetActive(false);
            }
        }
    }
}
