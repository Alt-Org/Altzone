using UnityEngine;
using Quantum;

namespace Battle.View.Player
{
    public class BattlePlayerViewController : QuantumCallbacks
    {
        [SerializeField] private GameObject _playerEntityGameObject;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventBattlePlayerViewInit>(this, OnViewInit);
        }

        private void OnViewInit(EventBattlePlayerViewInit e)
        {
            EntityRef entityRef = _playerEntityGameObject.GetComponent<QuantumEntityView>().EntityRef;
            if (entityRef != e.Entity) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
